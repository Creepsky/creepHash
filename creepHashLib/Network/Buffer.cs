/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MultiCryptoToolLib.Network
{
    public class Buffer
    {
        private readonly byte[] _data;

        public byte[] Data
        {
            get => ToArray();
            set
            {
                value.CopyTo(_data, 0);
                Length = value.Length;
            }
        }
        public int Length { get; private set; }

        public Buffer(int length)
        {
            _data = new byte[length];
            Length = 0;
        }

        public Buffer Receive(TcpClient client)
        {
            Length = client.GetStream().Read(_data, 0, _data.Length);
            return this;
        }

        public Buffer Send(TcpClient client)
        {
            if (Length > 0)
            {
                var stream = client.GetStream();
                stream.Write(_data, 0, Length);
            }
            
            return this;
        }

        public Buffer SetData(byte data) => SetData(new []{data});

        public Buffer SetData(string data) => SetData(Encoding.UTF8.GetBytes(data));

        public Buffer SetData(byte[] data)
        {
            Data = data;
            return this;
        }

        public byte[] ToArray() => Length > 0 ? _data.Take(Length).ToArray() : null;

        public string ToString(Encoding encoding) => encoding.GetString(_data, 0, Length);

        /// <inheritdoc />
        public override string ToString() => ToString(Encoding.UTF8);
    }
}