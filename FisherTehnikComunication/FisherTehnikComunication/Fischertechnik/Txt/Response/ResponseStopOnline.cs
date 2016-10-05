﻿using System;

namespace FactoryInterface.Fischertechnik.Txt.Response
{
    class ResponseStopOnline : ResponseBase
    {
        public ResponseStopOnline()
        {
            ResponseId = TxtInterface.ResponseIdStopOnline;
        }
        
        public override void FromByteArray(byte[] bytes)
        {
            if (bytes.Length < GetResponseLength())
            {
                throw new InvalidOperationException($"The byte array is to short to read. Length is {bytes.Length} bytes and {GetResponseLength()} bytes are needed");
            }


            var id = BitConverter.ToUInt32(bytes, 0);

            if (id != ResponseId)
            {
                throw new InvalidOperationException($"The byte array does not match the response id. Expected id is {ResponseId} and response id is {id}");
            }
        }
    }
}
