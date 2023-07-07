using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Analytics.Internal
{
    internal interface IBufferSystemCalls
    {
        string GenerateGuid();
        DateTime Now();

        bool CanAccessFileSystem();
        bool FileExists(string path);
        void WriteFile(string path, byte[] bytes);
        byte[] ReadFile(string path);
        void DeleteFile(string path);
    }

    class BufferSystemCalls : IBufferSystemCalls
    {
        public string GenerateGuid()
        {
            // TODO: investigate direct .ToByteArray()
            return Guid.NewGuid().ToString();
        }

        public DateTime Now()
        {
            return DateTime.Now;
        }

        public bool CanAccessFileSystem()
        {
            // Switch requires a specific setup to have write access to the disc so it won't be handled here.
            return
                Application.platform != RuntimePlatform.Switch &&
#if !UNITY_2021_1_OR_NEWER
                Application.platform != RuntimePlatform.XboxOne &&
#endif
#if UNITY_2019 || UNITY_2020_2_OR_NEWER
                Application.platform != RuntimePlatform.GameCoreXboxOne &&
                Application.platform != RuntimePlatform.GameCoreXboxSeries &&
                Application.platform != RuntimePlatform.PS5 &&
#endif
                Application.platform != RuntimePlatform.PS4 &&
                !String.IsNullOrEmpty(Application.persistentDataPath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void WriteFile(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public byte[] ReadFile(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }

    class BufferX : IBuffer
    {
        const long k_CacheFileMaximumSize = 1024 * 1024 * 5;
        readonly string k_CacheFilePath = $"{Application.persistentDataPath}/ugs_analytics_cache";
        const string k_SecondDateFormat = "yyyy-MM-dd HH:mm:ss zzz";
        const string k_MillisecondDateFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";
        static readonly string[] k_AllDateFormats = { k_SecondDateFormat, k_MillisecondDateFormat };

        readonly MemoryStream m_Buffer;
        readonly IBufferSystemCalls m_SystemCalls;

        public string UserID { get; set; }
        public string InstallID { get; set; }
        public string PlayerID { get; set; }
        public string SessionID { get; set; }

        public int Length => (int)m_Buffer.Length;

        /// <summary>
        /// The number of events that have been recorded into this buffer.
        /// </summary>
        internal int EventsRecorded { get; private set; }

        /// <summary>
        /// The raw contents of the underlying bytestream.
        /// Only exposed for unit testing.
        /// </summary>
        internal byte[] RawContents => m_Buffer.ToArray();

        public BufferX(IBufferSystemCalls eventIdGenerator)
        {
            m_Buffer = new MemoryStream();
            m_SystemCalls = eventIdGenerator;

            ClearBuffer();
        }

        private void WriteString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++)
            {
                m_Buffer.WriteByte(bytes[i]);
            }
        }

        public void PushStartEvent(string name, DateTime datetime, long? eventVersion, bool addPlayerIdsToEventBody)
        {
#if UNITY_ANALYTICS_EVENT_LOGS
            Debug.LogFormat("Recorded event {0} at {1} (UTC)", name, SerializeDateTime(datetime));
#endif
            WriteString("{");
            WriteString("\"eventName\":\"");
            WriteString(name);
            WriteString("\",");
            WriteString("\"userID\":\"");
            WriteString(UserID);
            WriteString("\",");
            WriteString("\"sessionID\":\"");
            WriteString(SessionID);
            WriteString("\",");
            WriteString("\"eventUUID\":\"");
            WriteString(m_SystemCalls.GenerateGuid());
            WriteString("\",");

            WriteString("\"eventTimestamp\":\"");
            WriteString(SerializeDateTime(datetime));
            WriteString("\",");

            if (eventVersion != null)
            {
                WriteString("\"eventVersion\":");
                WriteString(eventVersion.ToString());
                WriteString(",");
            }

            if (addPlayerIdsToEventBody)
            {
                WriteString("\"unityInstallationID\":\"");
                WriteString(InstallID);
                WriteString("\",");

                if (!String.IsNullOrEmpty(PlayerID))
                {
                    WriteString("\"unityPlayerID\":\"");
                    WriteString(PlayerID);
                    WriteString("\",");
                }
            }

            WriteString("\"eventParams\":{");
        }

        private void StripTrailingCommaIfNecessary()
        {
            // Stripping the comma once at the end of something is probably
            // faster than checking to see if we need to add one before
            // every single property inside it. Even though it seems
            // a bit convoluted.

            m_Buffer.Seek(-1, SeekOrigin.End);
            char precedingChar = (char)m_Buffer.ReadByte();
            if (precedingChar == ',')
            {
                // Burn that comma, we don't need it and it breaks JSON!
                m_Buffer.Seek(-1, SeekOrigin.Current);
                m_Buffer.SetLength(m_Buffer.Length - 1);
            }
        }

        public void PushEndEvent()
        {
            StripTrailingCommaIfNecessary();

            // Close params block, close event object, comma to prepare for next item
            WriteString("}},");

            EventsRecorded++;
        }

        public void PushObjectStart(string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString("{");
        }

        public void PushObjectEnd()
        {
            StripTrailingCommaIfNecessary();

            WriteString("},");
        }

        public void PushArrayStart(string name)
        {
            WriteString("\"");
            WriteString(name);
            WriteString("\":");
            WriteString("[");
        }

        public void PushArrayEnd()
        {
            StripTrailingCommaIfNecessary();

            WriteString("],");
        }

        public void PushDouble(double val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            var formatted = val.ToString(CultureInfo.InvariantCulture);
            WriteString(formatted);
            WriteString(",");
        }

        public void PushFloat(float val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            var formatted = val.ToString(CultureInfo.InvariantCulture);
            WriteString(formatted);
            WriteString(",");
        }

        public void PushString(string val, string name = null)
        {
#if UNITY_ANALYTICS_DEVELOPMENT
            Debug.AssertFormat(!String.IsNullOrEmpty(val), "Required to have a value");
#endif
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }

            // TODO: JsonConvert here is necessary to ensure strings don't break the JSON we are producing,
            // BUT it is also a major performance hotspot. Can we escape embedded JSON in a better way?
            WriteString(JsonConvert.ToString(val));
            WriteString(",");
        }

        public void PushInt64(long val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString(val.ToString());
            WriteString(",");
        }

        public void PushInt(int val, string name = null)
        {
            PushInt64(val, name);
        }

        public void PushBool(bool val, string name = null)
        {
            if (name != null)
            {
                WriteString("\"");
                WriteString(name);
                WriteString("\":");
            }
            WriteString(val ? "true" : "false");
            WriteString(",");
        }

        public void PushTimestamp(DateTime val, string name)
        {
            WriteString("\"");
            WriteString(name);
            WriteString("\":\"");
            WriteString(SerializeDateTime(val));
            WriteString("\",");
        }

        public void PushEvent(Event evt)
        {
            // Serialize event

            var dateTime = m_SystemCalls.Now();
            PushStartEvent(evt.Name, dateTime, evt.Version, false);

            // Serialize event params

            var eData = evt.Parameters;

            foreach (var data in eData.Data)
            {
                if (data.Value is float f32Val)
                {
                    PushFloat(f32Val, data.Key);
                }
                else if (data.Value is double f64Val)
                {
                    PushDouble(f64Val, data.Key);
                }
                else if (data.Value is string strVal)
                {
                    PushString(strVal, data.Key);
                }
                else if (data.Value is int intVal)
                {
                    PushInt(intVal, data.Key);
                }
                else if (data.Value is Int64 int64Val)
                {
                    PushInt64(int64Val, data.Key);
                }
                else if (data.Value is bool boolVal)
                {
                    PushBool(boolVal, data.Key);
                }
            }

            PushEndEvent();
        }

        public byte[] Serialize(List<Buffer.Token> tokens)
        {
            if (EventsRecorded > 0)
            {
                StripTrailingCommaIfNecessary();
                var end = m_Buffer.Position;

                WriteString("]}");
                var payload = m_Buffer.ToArray();

                // Reset the position and put the trailing comma back, so if we add new events,
                // they don't start after the footer. If this payload fails to upload, we will
                // want to include the newer events.
                m_Buffer.SetLength(end);
                m_Buffer.Position = end;
                WriteString(",");

                return payload;
            }
            else
            {
                return null;
            }
        }

        public void ClearBuffer()
        {
            m_Buffer.SetLength(0);
            m_Buffer.Position = 0;

            EventsRecorded = 0;

            WriteString("{\"eventList\":[");
        }

        //
        // TODO: disk access?
        //

        public void FlushToDisk()
        {
            if (m_SystemCalls.CanAccessFileSystem())
            {
                ClearDiskCache();

                if (EventsRecorded > 0)
                {
                    m_SystemCalls.WriteFile(k_CacheFilePath, m_Buffer.ToArray());
                }
            }
        }

        public void ClearDiskCache()
        {
            if (m_SystemCalls.CanAccessFileSystem() &&
                m_SystemCalls.FileExists(k_CacheFilePath))
            {
                m_SystemCalls.DeleteFile(k_CacheFilePath);
            }
        }

        public void LoadFromDisk()
        {
            if (m_SystemCalls.CanAccessFileSystem() &&
                m_SystemCalls.FileExists(k_CacheFilePath))
            {
                // NOTE: we are NOT calling ClearBuffer because we are completely overwriting it
                // with the file contents. The file will include the payload header.
                m_Buffer.SetLength(0);
                m_Buffer.Position = 0;

                var cacheFile = m_SystemCalls.ReadFile(k_CacheFilePath);
                m_Buffer.Write(cacheFile, 0, cacheFile.Length);

                // We do not store any event metadata in the file, so we can't actually know how many events there are.
                // However, we will only create a file at all if there is one event or more, so setting this
                // value to "greater than zero" is really all that matters.
                // TODO: Could store a single-byte count prefix?
                EventsRecorded = 1;
            }
        }

        static string SerializeDateTime(DateTime dateTime)
        {
            return dateTime.ToString(k_MillisecondDateFormat, CultureInfo.InvariantCulture);
        }

        //
        // TODO: bin these during installation and/or replace with modern equivalents
        //

        public List<Buffer.Token> CloneTokens()
        {
            throw new NotImplementedException();
        }

        public void InsertTokens(List<Buffer.Token> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
