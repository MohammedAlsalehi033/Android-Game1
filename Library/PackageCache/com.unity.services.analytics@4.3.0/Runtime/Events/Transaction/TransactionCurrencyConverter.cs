using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Analytics
{
    class TransactionCurrencyConverter
    {
        Dictionary<string, int> m_CurrencyCodeToMinorUnits;

        public long Convert(string currencyCode, double value)
        {
            if (String.IsNullOrEmpty(currencyCode))
            {
                throw new ArgumentNullException(nameof(currencyCode), "Currency code cannot be null or empty.");
            }

            if (m_CurrencyCodeToMinorUnits == null)
            {
                LoadCurrencyCodeDictionary();
            }

            var currencyCodeUppercased = currencyCode.ToUpperInvariant();

            if (m_CurrencyCodeToMinorUnits.TryGetValue(currencyCodeUppercased, out int numberOfMinorUnits))
            {
                return (long)(value * Math.Pow(10, numberOfMinorUnits));
            }
            else
            {
                Debug.LogWarning("Unknown currency provided to convert method, no conversion will be performed and returned value will be 0.");
                return 0;
            }
        }

        public void LoadCurrencyCodeDictionary()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("iso4217");
            if (textAsset)
            {
                string text = textAsset.text;
                if (String.IsNullOrEmpty(text))
                {
                    Debug.LogWarning("Error loading currency definitions, no conversions will be performed.");
                    m_CurrencyCodeToMinorUnits = new Dictionary<string, int>();
                }
                else
                {
                    try
                    {
                        m_CurrencyCodeToMinorUnits = JsonConvert.DeserializeObject<Dictionary<string, int>>(text);
                    }
                    catch (JsonException e)
                    {
                        Debug.LogWarning($"Failed to deserialize JSON for currency conversion, no conversions will be performed");
                        Debug.LogWarning(e.Message);
                        m_CurrencyCodeToMinorUnits = new Dictionary<string, int>();
                    }
                }
            }
        }
    }
}
