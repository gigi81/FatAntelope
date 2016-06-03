using System;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using FatAntelope;

namespace FatAntelope.Tests
{
    [TestClass]
    public class XDiffTest
    {
        const string Document1 = @"
                <root>
                    <child1 name1='child1' type1='elem1'>child1</child1>
                    <child2 name1='child2' type1='elem2'>child2</child2>
                </root>";

        /// <summary>
        /// Same as <see cref="Document1"/> but with different order (should be a match if compared with <see cref="Document1"/>)
        /// </summary>
        const string Document2 = @"
                <root>
                    <child2 name1='child2' type1='elem2'>child2</child2>
                    <child1 name1='child1' type1='elem1'>child1</child1>
                </root>";

        /// <summary>
        /// Similar to <see cref="Document1"/> but with different content
        /// </summary>
        const string Document3 = @"
                <root>
                    <child1 name1='child1' type1='elem1'>child1</child1>
                    <child2 name1='child2' type1='elem2'>different</child2>
                </root>";

        private static readonly string Filename1 = Path.GetTempFileName();
        private static readonly string Filename2 = Path.GetTempFileName();
        private static readonly string Filename3 = Path.GetTempFileName();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            File.WriteAllText(Filename1, Document1);
            File.WriteAllText(Filename2, Document2);
            File.WriteAllText(Filename3, Document3);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteFile(Filename1);
            DeleteFile(Filename2);
            DeleteFile(Filename3);
        }

        private static void DeleteFile(string filename)
        {
            if(File.Exists(filename))
                File.Delete(filename);
        }

        [TestMethod]
        public void AttributeMarkedChanged()
        {
            var doc1 = new XmlDocument();
            doc1.LoadXml(@"
                <root>
                    <child1 name1='child1' type1='elem1'>child1</child1>
                    <child2 name1='child2' type1='elem2'>child2</child2>
                </root>"
            );

            // reordered xml but same values
            var doc2 = new XmlDocument();
            doc2.LoadXml(@"
                <root>
                    <child1 type1='elem1' name1='DIFFERENT'>child1</child1>
                    <child2 name1='child2' type1='elem2'>child2</child2>
                </root>"
            );

            var tree1 = new XTree(doc1);
            var tree2 = new XTree(doc2);
            XDiff.Diff(tree1, tree2);

            Assert.AreEqual(tree1.Root.Match, MatchType.Change);
            Assert.AreEqual(tree2.Root.Match, MatchType.Change);

            Assert.AreEqual(tree1.Root.Children.Length, 2);
            Assert.AreEqual(tree2.Root.Children.Length, 2);

            Assert.AreEqual(tree1.Root.Elements.Length, 2);
            Assert.AreEqual(tree2.Root.Elements.Length, 2);

            Assert.AreEqual(tree1.Root.Elements[0].Match, MatchType.Change);
            Assert.AreEqual(tree2.Root.Elements[0].Match, MatchType.Change);

            Assert.AreEqual(tree1.Root.Elements[1].Match, MatchType.Match);
            Assert.AreEqual(tree2.Root.Elements[1].Match, MatchType.Match);

            Assert.AreEqual(tree1.Root.Elements[0].Attributes.Length, 2);
            Assert.AreEqual(tree2.Root.Elements[0].Attributes.Length, 2);

            Assert.AreEqual(tree1.Root.Elements[0].Texts.Length, 1);
            Assert.AreEqual(tree2.Root.Elements[0].Texts.Length, 1);

            Assert.AreEqual(tree1.Root.Elements[0].Texts[0].Match, MatchType.Match);
            Assert.AreEqual(tree2.Root.Elements[0].Texts[0].Match, MatchType.Match);

            Assert.AreEqual(tree1.Root.Elements[0].Attributes[0].Match, MatchType.Change);
            Assert.AreEqual(tree2.Root.Elements[0].Attributes[0].Match, MatchType.Match);

            Assert.AreEqual(tree1.Root.Elements[0].Attributes[1].Match, MatchType.Match);
            Assert.AreEqual(tree2.Root.Elements[0].Attributes[1].Match, MatchType.Change);

            Assert.AreEqual(tree1.Root.Elements[0].Attributes[0].Matching, tree2.Root.Elements[0].Attributes[1]);
        }

        [TestMethod]
        public void DocumentEqualsToItself()
        {
            Assert.IsTrue(XDiff.Equals(Filename1, Filename1));
        }

        [TestMethod]
        public void Document1EqualsToDocument2()
        {
            Assert.IsTrue(XDiff.Equals(Filename1, Filename2));
        }

        [TestMethod]
        public void Document1NotEqualsToDocument3()
        {
            Assert.IsFalse(XDiff.Equals(Filename1, Filename3));
        }
    }
}
