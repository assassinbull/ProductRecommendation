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
    public class ResponseBase<T>
    {
        #region | Ctor |

        public ResponseBase()
        {
            this.Status = ServiceResponseStatuses.Success;
            Messages = new Dictionary<string, string>();
        }

        #endregion

        #region  | Properties |

        [DataMember]
        public ServiceResponseStatuses Status { get; set; }

        [DataMember]
        public T Data { get; set; }

        [DataMember]
        public Dictionary<string, string> Messages { get; set; }

        #endregion

        #region | Helper Methods |
        public static ResponseBase<T> CreateResponse(T data)
        {
            var response = new ResponseBase<T> { Status = ServiceResponseStatuses.Success, Data = data };
            return response;
        }

        public void AddMessage(string messageKey, string message)
        {
            if (this.Messages == null)
            {
                this.Messages = new Dictionary<string, string>();
            }

            bool alreadyExists = this.Messages
                .Where(eachMessage => string.Compare(eachMessage.Key, messageKey, true) == 0)
                .Count() > 0;

            if (!alreadyExists)
            {
                this.Messages.Add(messageKey, message);
            }
        }

        public bool IsSuccessful()
        {
            return this != null && this.Status == ServiceResponseStatuses.Success;
        }

        #region | Error |

        public void Error()
        {
            this.Status = ServiceResponseStatuses.Error;
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
            this.Status = ServiceResponseStatuses.Success;
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
            this.Status = ServiceResponseStatuses.Warning;
        }

        public void Warning(T data, Dictionary<string, string> messageList)
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

    [Serializable]
    [DataContract]
    public enum ServiceResponseStatuses
    {
        [EnumMember]
        Error,

        [EnumMember]
        Success,

        [EnumMember]
        Warning
    }
}