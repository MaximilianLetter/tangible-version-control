using UnityEngine;

namespace DebugStuff
{
    public class DebugLog : MonoBehaviour
    {
        //#if !UNITY_EDITOR
        static string myLog = "";
        private string output;
        private string stack;

        private TMPro.TMP_Text textField;

        private void Awake()
        {
            textField = GetComponent<TMPro.TMP_Text>();
        }

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            output = logString;
            stack = stackTrace;
            myLog = output + "\n" + myLog;
            if (myLog.Length > 5000)
            {
                myLog = myLog.Substring(0, 4000);
            }
        }

        private void Update()
        {
            textField.text = myLog;
        }
    }
}