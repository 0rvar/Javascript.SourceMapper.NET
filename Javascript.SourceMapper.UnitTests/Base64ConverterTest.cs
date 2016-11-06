using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Javascript.SourceMapper.UnitTests
{
    [TestClass]
    public class Base64ConverterTest
    {

        [TestMethod]
        public void TestEncodingBase64()
        {
            Assert.AreEqual('A', Base64Converter.Encode(0));
            Assert.AreEqual('W', Base64Converter.Encode(22));
            Assert.AreEqual('q', Base64Converter.Encode(42));
            Assert.AreEqual('3', Base64Converter.Encode(55));
            Assert.AreEqual('/', Base64Converter.Encode(63));
        }

        [TestMethod]
        public void TestDecodingBase64()
        {
            
            Assert.AreEqual(0, Base64Converter.Decode('A'));
            Assert.AreEqual(22, Base64Converter.Decode('W'));
            Assert.AreEqual(42, Base64Converter.Decode('q'));
            Assert.AreEqual(55, Base64Converter.Decode('3'));
            Assert.AreEqual(63, Base64Converter.Decode('/'));
            Assert.AreEqual(62, Base64Converter.Decode('+'));
        }

        [TestMethod]
        public void TestEncodingAndDecodingBase64()
        {
            for(var i = 0; i < 64; i++)
            {
                Assert.AreEqual(i, Base64Converter.Decode(Base64Converter.Encode(i)));
            }

            foreach(var i in Base64Converter.CHARACTER_MAP)
            {
                Assert.AreEqual(i, Base64Converter.Encode(Base64Converter.Decode(i)));
            }
        }
    }
}
