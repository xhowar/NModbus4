using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Modbus.Data;

namespace Modbus.Message
{
    internal class WriteSingleRegisterRequestResponse : ModbusMessageWithData<RegisterCollection>, IModbusRequest
    {
        public WriteSingleRegisterRequestResponse()
        {
        }

        public WriteSingleRegisterRequestResponse(byte slaveAddress, ushort startAddress, ushort registerValue)
            : base(slaveAddress, Modbus.WriteSingleRegister)
        {
            StartAddress = startAddress;
            Data = new RegisterCollection(registerValue);
        }

        public override int MinimumFrameSize
        {
            get { return 6; }
        }

        public ushort StartAddress
        {
            get { return MessageImpl.StartAddress.Value; }
            set { MessageImpl.StartAddress = value; }
        }

        public override string ToString()
        {
            Debug.Assert(Data != null, "Argument Data cannot be null.");
            Debug.Assert(Data.Count() == 1, "Data should have a count of 1.");

            return String.Format(CultureInfo.InvariantCulture, "Write single holding register {0} at address {1}.",
                Data[0], StartAddress);
        }

        public void ValidateResponse(IModbusMessage response)
        {
            var typedResponse = (WriteSingleRegisterRequestResponse) response;

            if (StartAddress != typedResponse.StartAddress)
            {
                throw new IOException(String.Format(CultureInfo.InvariantCulture,
                    "Unexpected start address in response. Expected {0}, received {1}.",
                    StartAddress,
                    typedResponse.StartAddress));
            }

            if (Data.First() != typedResponse.Data.First())
            {
                throw new IOException(String.Format(CultureInfo.InvariantCulture,
                    "Unexpected data in response. Expected {0}, received {1}.",
                    Data.First(),
                    typedResponse.Data.First()));
            }
        }

        protected override void InitializeUnique(byte[] frame)
        {
            StartAddress = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
            Data = new RegisterCollection((ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4)));
        }
    }
}