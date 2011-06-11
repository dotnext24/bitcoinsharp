/**
 * Copyright 2011 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace BitCoinSharp
{
    public class VarInt
    {
        public long Value { get; private set; }

        public VarInt(long value)
        {
            Value = value;
        }

        // BitCoin has its own varint format, known in the C++ source as "compact size".
        public VarInt(byte[] buf, int offset)
        {
            var first = 0xFF & buf[offset];
            long val;
            if (first < 253)
            {
                // 8 bits.
                val = first;
            }
            else if (first == 253)
            {
                // 16 bits.
                val = (0xFF & buf[offset + 1]) | ((0xFF & buf[offset + 2]) << 8);
            }
            else if (first == 254)
            {
                // 32 bits.
                val = Utils.ReadUint32(buf, offset + 1);
            }
            else
            {
                // 64 bits.
                val = Utils.ReadUint32(buf, offset + 1) | (Utils.ReadUint32(buf, offset + 5) << 32);
            }
            Value = val;
        }

        public int SizeInBytes
        {
            get
            {
                // Java doesn't have the actual value of MAX_INT, as all types in Java are signed.
                if (Utils.IsLessThanUnsigned(Value, 253))
                    return 1;
                if (Utils.IsLessThanUnsigned(Value, 65536))
                    return 3; // 1 marker + 2 data bytes
                if (Utils.IsLessThanUnsigned(Value, 4294967296L))
                    return 5; // 1 marker + 4 data bytes
                return 9; // 1 marker + 8 data bytes
            }
        }

        public byte[] Encode()
        {
            return EncodeBe();
        }

        public byte[] EncodeBe()
        {
            if (Utils.IsLessThanUnsigned(Value, 253))
            {
                return new[] {(byte) Value};
            }
            if (Utils.IsLessThanUnsigned(Value, 65536))
            {
                return new[] {(byte) 253, (byte) Value, (byte) (Value >> 8)};
            }
            if (Utils.IsLessThanUnsigned(Value, 4294967295L))
            {
                var bytes = new byte[5];
                bytes[0] = 254;
                Utils.Uint32ToByteArrayLe(Value, bytes, 1);
                return bytes;
            }
            else
            {
                var bytes = new byte[9];
                bytes[0] = 255;
                Utils.Uint32ToByteArrayLe(Value, bytes, 1);
                Utils.Uint32ToByteArrayLe(Value >> 32, bytes, 5);
                return bytes;
            }
        }
    }
}