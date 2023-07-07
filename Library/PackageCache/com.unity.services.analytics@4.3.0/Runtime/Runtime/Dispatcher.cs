using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Services.Analytics.Internal
{
    interface IDispatcher
    {
        string CollectUrl { get; set; }
        Task Flush();
    }

    class Dispatcher : IDispatcher
    {
        readonly IBuffer m_DataBuffer;
        readonly IWebRequestHelper m_WebRequestHelper;

        internal bool FlushInProgress { get; private set; }
        IWebRequest m_FlushRequest;
        List<Buffer.Token> m_FlushPayload;

        public string CollectUrl { get; set; }

        IConsentTracker ConsentTracker { get; }

        public Dispatcher(IBuffer buffer, IWebRequestHelper webRequestHelper, IConsentTracker consentTracker = null)
        {
            m_DataBuffer = buffer;
            m_WebRequestHelper = webRequestHelper;
            ConsentTracker = consentTracker;
        }

        public async Task Flush()
        {
            if (FlushInProgress)
            {
                Debug.LogWarning("Analytics Dispatcher is already flushing.");
                return;
            }

            // Also, check if the consent was definitely checked and given at this point.
            if (!ConsentTracker.IsGeoIpChecked() || !ConsentTracker.IsConsentGiven())
            {
                Debug.LogWarning("Required consent wasn't checked and given when trying to dispatch events, the events cannot be sent.");
                return;
            }

            FlushInProgress = true;

            await FlushBufferToService();
        }

        async Task FlushBufferToService()
        {
            // Serialize it into a JSON Blob, then POST it to the Collect bulk URL.
            // 'Bulk Events' -> https://docs.deltadna.com/advanced-integration/rest-api/

            var tokens = m_DataBuffer.CloneTokens();

#if UNITY_WEBGL
            // NOTE: we are maintaining the async/Task format, even though this is now synchronous,
            // to minimise the fallout of this platform-specific path. If we made it fully synchronous all
            // the way up, we would have to change several method signatures too, which could be Breaking.
            var task = Task.FromResult(m_DataBuffer.Serialize(tokens));
#else
            var task = Task.Factory.StartNew(() => m_DataBuffer.Serialize(tokens));
#endif
            var postBytes = await task;

            if (postBytes == null || postBytes.Length == 0)
            {
                FlushInProgress = false;
                m_FlushPayload = null;
                return;
            }

            m_FlushRequest = m_WebRequestHelper.CreateWebRequest(CollectUrl, UnityWebRequest.kHttpVerbPOST, postBytes);

            if (ConsentTracker.IsGeoIpChecked() && ConsentTracker.IsConsentGiven())
            {
                foreach (var header in ConsentTracker.requiredHeaders)
                {
                    m_FlushRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            m_FlushPayload = tokens;

            m_WebRequestHelper.SendWebRequest(m_FlushRequest, UploadCompleted);

#if UNITY_ANALYTICS_EVENT_LOGS
            Debug.Log("Uploading events...");
#endif
        }

        void UploadCompleted(long responseCode)
        {
            // Callback
            // If the result is successful we will remove the request.
            // else if there was a failure, we insert the tokens back into the buffer.

            if (!m_FlushRequest.isNetworkError &&
                (responseCode == 204 || responseCode == 400))
            {
                // If we get a 400 response code, the JSON is malformed which means we have a bad event somewhere
                // in the queue. In this case, our only recourse is to discard the tokens. If they were
                // to be reinserted into the queue, the bad event would recur forever and no more data would ever
                // by uploaded.
                // So, slightly counter-intuitively, our actions on getting a success 204 and an error 400 are actually the same.
                // Other errors are likely transient and should not result in clearance of the buffer.

                m_DataBuffer.ClearDiskCache();

#if UNITY_ANALYTICS_EVENT_LOGS
                if (responseCode == 204)
                {
                    Debug.Log("Events uploaded successfully!");
                }
                else
                {
                    Debug.Log("Events upload failed due to malformed JSON, likely from corrupt event. Event buffer has been cleared.");
                }
#endif
            }
            else
            {
                // Reinsert the tokens back into the buffer.
                m_DataBuffer.InsertTokens(m_FlushPayload);
                m_DataBuffer.FlushToDisk();

#if UNITY_ANALYTICS_EVENT_LOGS
                if (m_FlushRequest.isNetworkError)
                {
                    Debug.Log("Events failed to upload (network error) -- will retry at next heartbeat.");
                }
                else
                {
                    Debug.LogFormat("Events failed to upload (code {0}) -- will retry at next heartbeat.", responseCode);
                }
#endif
            }

            // Clear the request now that we are done.
            FlushInProgress = false;
            m_FlushPayload = null;
            m_FlushRequest.Dispose();
            m_FlushRequest = null;
        }
    }
}
