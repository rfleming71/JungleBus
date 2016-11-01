// <copyright file="JungleBusConfigurationException.cs">
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
namespace JungleBus.Interfaces.Exceptions
{
    /// <summary>
    /// An issue with the configuration as occurred
    /// </summary>
    public class JungleBusConfigurationException : JungleBusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JungleBusConfigurationException" /> class.
        /// </summary>
        /// <param name="configurationSetting">Setting that caused the exception</param>
        /// <param name="message">Error message</param>
        public JungleBusConfigurationException(string configurationSetting, string message)
            : base(string.Format("Setting: {0} Message: {1}", configurationSetting, message))
        {
        }
    }
}
