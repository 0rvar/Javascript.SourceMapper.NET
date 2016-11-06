using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Javascript.SourceMapper.UnitTests
{
    [TestClass]
    public class Base64VLQConverterTest
    {
        [TestMethod]
        public void TestToVlq()
        {
            Assert.AreEqual(2, Base64VLQConverter.toVLQ(1));
            Assert.AreEqual(3, Base64VLQConverter.toVLQ(-1));
            Assert.AreEqual(4, Base64VLQConverter.toVLQ(2));
            Assert.AreEqual(5, Base64VLQConverter.toVLQ(-2));
        }

        [TestMethod]
        public void TestFromVlq()
        {
            Assert.AreEqual(1, Base64VLQConverter.fromVLQ(2));
            Assert.AreEqual(-1, Base64VLQConverter.fromVLQ(3));
            Assert.AreEqual(2, Base64VLQConverter.fromVLQ(4));
            Assert.AreEqual(-2, Base64VLQConverter.fromVLQ(5));
        }

        [TestMethod]
        public void TestToVlqAndBack()
        {
            for(int i = -1000; i < 1000; i++)
            {
                Assert.AreEqual(i, Base64VLQConverter.fromVLQ(Base64VLQConverter.toVLQ(i)));
            }
        }

        [TestMethod]
        public void TestEncodingBase64VLQ()
        {
            Assert.AreEqual("A", Base64VLQConverter.Encode(0));
            Assert.AreEqual("C", Base64VLQConverter.Encode(1));
            Assert.AreEqual("D", Base64VLQConverter.Encode(-1));

            Assert.AreEqual("hkh9B", Base64VLQConverter.Encode(-1000000));
            Assert.AreEqual("ruyH", Base64VLQConverter.Encode(-124133));
            Assert.AreEqual("5iY", Base64VLQConverter.Encode(-12332));
            Assert.AreEqual("9qE", Base64VLQConverter.Encode(-2222));
            Assert.AreEqual("3iD", Base64VLQConverter.Encode(-1579));
            Assert.AreEqual("jE", Base64VLQConverter.Encode(-65));
            Assert.AreEqual("zB", Base64VLQConverter.Encode(-25));
            Assert.AreEqual("pB", Base64VLQConverter.Encode(-20));
            Assert.AreEqual("X", Base64VLQConverter.Encode(-11));
            Assert.AreEqual("T", Base64VLQConverter.Encode(-9));
            Assert.AreEqual("F", Base64VLQConverter.Encode(-2));
            Assert.AreEqual("O", Base64VLQConverter.Encode(7));
            Assert.AreEqual("e", Base64VLQConverter.Encode(15));
            Assert.AreEqual("uB", Base64VLQConverter.Encode(23));
            Assert.AreEqual("wF", Base64VLQConverter.Encode(88));
            Assert.AreEqual("suC", Base64VLQConverter.Encode(1254));
            Assert.AreEqual("67E", Base64VLQConverter.Encode(2493));
            Assert.AreEqual("+1uB", Base64VLQConverter.Encode(23903));
            Assert.AreEqual("u28H", Base64VLQConverter.Encode(129383));
            Assert.AreEqual("k1mS", Base64VLQConverter.Encode(298322));
            Assert.AreEqual("gkh9B", Base64VLQConverter.Encode(1000000));
        }

        [TestMethod]
        public void TestDecodingBase64VLQ()
        {
            Assert.AreEqual(-1000000, Base64VLQConverter.Decode("hkh9B").Result);
            Assert.AreEqual(-124133, Base64VLQConverter.Decode("ruyH").Result);
            Assert.AreEqual(-12332, Base64VLQConverter.Decode("5iY").Result);
            Assert.AreEqual(-2222, Base64VLQConverter.Decode("9qE").Result);
            Assert.AreEqual(-1579, Base64VLQConverter.Decode("3iD").Result);
            Assert.AreEqual(-65, Base64VLQConverter.Decode("jE").Result);
            Assert.AreEqual(-25, Base64VLQConverter.Decode("zB").Result);
            Assert.AreEqual(-20, Base64VLQConverter.Decode("pB").Result);
            Assert.AreEqual(-11, Base64VLQConverter.Decode("X").Result);
            Assert.AreEqual(-9, Base64VLQConverter.Decode("T").Result);
            Assert.AreEqual(-2, Base64VLQConverter.Decode("F").Result);
            Assert.AreEqual(-1, Base64VLQConverter.Decode("D").Result);
            Assert.AreEqual(0, Base64VLQConverter.Decode("A").Result);
            Assert.AreEqual(1, Base64VLQConverter.Decode("C").Result);
            Assert.AreEqual(7, Base64VLQConverter.Decode("O").Result);
            Assert.AreEqual(15, Base64VLQConverter.Decode("e").Result);
            Assert.AreEqual(23, Base64VLQConverter.Decode("uB").Result);
            Assert.AreEqual(88, Base64VLQConverter.Decode("wF").Result);
            Assert.AreEqual(1254, Base64VLQConverter.Decode("suC").Result);
            Assert.AreEqual(2493, Base64VLQConverter.Decode("67E").Result);
            Assert.AreEqual(23903, Base64VLQConverter.Decode("+1uB").Result);
            Assert.AreEqual(129383, Base64VLQConverter.Decode("u28H").Result);
            Assert.AreEqual(298322, Base64VLQConverter.Decode("k1mS").Result);
            Assert.AreEqual(1000000, Base64VLQConverter.Decode("gkh9B").Result);
        }
    }
}
