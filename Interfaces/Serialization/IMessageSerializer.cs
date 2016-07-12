// <copyright file="IMessageSerializer.cs">
//     The MIT License (MIT)
//
// Copyright(c) 2016 Ryan Fleming
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
using System;

namespace JungleBus.Interfaces.Serialization
{
    /// <summary>
    /// Serializes and deserializes message for transport over the bus
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serializes a message so if can be sent over the bus
        /// </summary>
        /// <param name="message">Message to serialize</param>
        /// <returns>Serialized object</returns>
        string Serialize(object message);

        /// <summary>
        /// Deserializes an object to type type
        /// </summary>
        /// <param name="message">Value to deserialize</param>
        /// <param name="type">Type to deserialize to</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(string message, Type type);
    }
}
