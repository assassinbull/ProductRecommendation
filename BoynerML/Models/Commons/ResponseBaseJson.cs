using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BoynerML.Models.Commons
{
    [Serializable]
    [DataContract(Name = "{0}")]
    public class ResponseBaseJson<T>
    {
        #region | Ctor |

        public ResponseBaseJson()
        {
            this.Status = ResponseSuccess;
            Messages = new List<ResponseMessage>();
        }

        #endregion

        #region  | Properties |

        private const string ResponseError = "error";
        private const string ResponseSuccess = "success";
        private const string ResponseWarning = "warning";

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public T Data { get; set; }

        [DataMember]
        public List<ResponseMessage> Messages { get; set; }

        #endregion

        #region | Helper Methods |
        public static ResponseBaseJson<T> CreateResponse(T data)
        {
            var response = new ResponseBaseJson<T> { Status = ResponseSuccess, Data = data };
            return response;
        }

        public void AddMessage(string messageKey, string message)
        {
            if (this.Messages == null)
            {
                this.Messages = new List<ResponseMessage>();
            }

            bool alreadyExists = this.Messages
                .Where(eachMessage => string.Compare(eachMessage.Key, messageKey, true) == 0)
                .Count() > 0;

            if (!alreadyExists)
            {
                this.Messages.Add(new ResponseMessage { Key = messageKey, Value = message });
            }
        }

        public bool IsSuccessful()
        {
            return this != null && this.Status == ResponseSuccess;
        }

        #region | Error |

        public void Error()
        {
            this.Status = ResponseError;
        }

        public void Error(string messageKey, string message)
        {
            this.Error();
            this.AddMessage(messageKey, message);
        }

        public void Error(Exception ex)
        {
            this.Error();
            this.AddMessage(GetCurrentMethodName(4), ex.Message);
        }

        public void Error(Dictionary<string, string> messages)
        {
            this.Error();
            foreach (KeyValuePair<string, string> keyValuePair in messages)
            {
                this.AddMessage(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public bool ValidData(object data, string dataName)
        {
            if (data == null)
            {
                this.Error(string.Format("{0}-not-found", dataName), string.Format("{0} does not exist!", dataName));
                return false;
            }
            else
                return true;
        }

        #endregion

        #region | Success |

        public void Success()
        {
            this.Status = ResponseSuccess;
        }

        public void Success(T data)
        {
            this.Data = data;
            this.Success();
        }

        public void Success(T data, bool clearMessages)
        {
            this.Data = data;
            if (clearMessages)
            {
                this.Messages.Clear();
            }
            this.Success();
        }

        #endregion

        #region | Warning |

        public void Warning()
        {
            this.Status = ResponseWarning;
        }

        public void Warning(T data, List<ResponseMessage> messageList)
        {
            this.Data = data;
            this.Messages = messageList;
            this.Warning();
        }

        public void Warning(T data, string messageKey, string message)
        {
            this.Data = data;
            this.AddMessage(messageKey, message);
            this.Warning();
        }

        #endregion

        public static string GetCurrentMethodName(int upperLevel)
        {
            string retval = null;
            StackTrace stackTrace = new StackTrace();
            var reflectedType = stackTrace.GetFrame(upperLevel).GetMethod().ReflectedType;
            if (reflectedType != null)
                retval = reflectedType.FullName + "." + stackTrace.GetFrame(upperLevel).GetMethod().Name;
            return retval;
        }

        #endregion
    }

    public class ResponseMessage
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}