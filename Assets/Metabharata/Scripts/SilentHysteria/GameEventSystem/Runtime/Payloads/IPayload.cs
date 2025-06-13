using System;

namespace NyxMachina.Shared.EventFramework.Core.Payloads
{
    /// <summary>
    /// Payload
    /// </summary>
    public interface IPayload
    {
        
    }

    public static class PayloadExtension
    {
        public static void ResetInvokeCount(this IPayloadInvokeState payload)
        {
            payload.LastInvokeCount = 0;
            payload.CurrentInvokeCount = 0;
        }

        public static void SetInvokeCount(this IPayloadInvokeState payload, int count)
        {
            payload.LastInvokeCount = count;
            payload.CurrentInvokeCount = count;
        }

        public static bool IsInvoked(this IPayloadInvokeState payload)
        {
            if (payload == null)
                return false;
            if (payload.CurrentInvokeCount <= payload.LastInvokeCount)
                return false;
            payload.LastInvokeCount = payload.CurrentInvokeCount;
            return true;

        }

        public static void Publish(this IPayload payload)
        {
            EventMessenger.Main.Publish(payload);
        }
    }
}