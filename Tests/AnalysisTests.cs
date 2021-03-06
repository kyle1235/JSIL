﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class AnalysisTests : GenericTestFixture {
        [Test]
        public void FieldAssignmentDetection () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\FieldAssignmentDetection.cs",
                "ct=1, mc=(a=0 b=0)\r\nct=1, mc=(a=2 b=1)\r\nct=3, mc=(a=2 b=1)",
                "ct=1, mc=(a=0 b=0)\r\nct=1, mc=(a=2 b=1)\r\nct=3, mc=(a=2 b=1)"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"mc.UpdateWithNewState(2, ct);"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @"mc.UpdateWithNewState(2, ct.MemberwiseClone());"
            ));
        }

        [Test]
        public void ReturnStructArgument () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnStructArgument.cs",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2",
                "a=2, b=1\r\na=2, b=2\r\na=3, b=2"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"(\$asm([0-9A-F])*).Program.ReturnArgument\(a.MemberwiseClone\(\)\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnArgument\(a\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void ReturnMutatedStructArgument () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnMutatedStructArgument.cs",
                "a=2, b=1\r\na=2, b=4\r\na=3, b=4",
                "a=2, b=1\r\na=2, b=4\r\na=3, b=4"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"(\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void ReturnMutatedNestedStruct () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\ReturnMutatedNestedStruct.cs",
                "a=2, b=1\r\na=2, b=4\r\na=3, b=4",
                "a=2, b=1\r\na=2, b=4\r\na=3, b=4"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(Regex.IsMatch(
                generatedJs,
                @"(\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a.MemberwiseClone\(\), 0\)"
            ));
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnMutatedArgument\(a, 0\).MemberwiseClone\(\);"
            ));
        }

        [Test]
        public void IncrementArgumentField () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\IncrementArgumentField.cs",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".IncrementArgumentValue(a)"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @".IncrementArgumentValue(a.MemberwiseClone())"
            ));
        }

        [Test]
        public void MutateNestedStruct () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\MutateNestedStruct.cs",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3",
                "a=2, b=1\r\na=2, b=1\r\na=2, b=3\r\na=3, b=3"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".IncrementArgumentValue(a)"
            ));
            Assert.IsTrue(generatedJs.Contains(
                @".IncrementArgumentValue(a.MemberwiseClone())"
            ));
        }

        [Test]
        public void PureStructOperator () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\PureStructOperator.cs",
                "a=1, b=2, c=3\r\na=1, b=2, c=3\r\n4",
                "a=1, b=2, c=3\r\na=1, b=2, c=3\r\n4"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".op_Addition(a.MemberwiseClone()"
            ), "Argument to op_Addition was cloned");
            Assert.IsFalse(generatedJs.ContainsRegex(
                @"\.op_Addition\([^\)]*\)\.MemberwiseClone\("
            ), "Return value of op_Addition was cloned");
            Assert.IsFalse(generatedJs.Contains(
                @"b.MemberwiseClone())"
            ), "Argument to op_Addition was cloned");
            Assert.IsFalse(generatedJs.Contains(
                @"c.MemberwiseClone())"
            ), "Argument to op_Addition was cloned");
            Assert.IsTrue(generatedJs.Contains(
                @".op_Addition(a, b)"
            ));
        }

        [Test]
        public void CopyGetEnumerator () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\CopyGetEnumerator.cs",
                "1\r\n2\r\n3",
                "1\r\n2\r\n3"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @"GetEnumerator().MemberwiseClone()"
            ), "The enumerator was cloned");
        }

        [Test]
        public void NestedReturnNew () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\NestedReturnNew.cs",
                "a=1, b=2, c=3, d=6\r\na=1, b=2, c=3, d=6",
                "a=1, b=2, c=3, d=6\r\na=1, b=2, c=3, d=6"
            );

            Console.WriteLine(generatedJs);
            Assert.IsFalse(generatedJs.Contains(
                @".MemberwiseClone()"
            ), "A struct was cloned");
        }

        [Test]
        public void NestedReturn () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\NestedReturn.cs",
                "a=1, b=2\r\na=3, b=2\r\na=3, b=3",
                "a=1, b=2\r\na=3, b=2\r\na=3, b=3"
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(Regex.IsMatch(
                generatedJs,
                @"b = (\$asm([0-9A-F])*).Program.ReturnArgument\((\$asm([0-9A-F])*)." +
                @"Program.ReturnIncrementedArgument\((\$asm([0-9A-F])*).Program.ReturnArgument\(a\)." +
                @"MemberwiseClone\(\)\)\).MemberwiseClone\(\)"
            ));
        }

        [Test]
        public void StructTemporaries () {
            var generatedJs = GenericTest(
                @"AnalysisTestCases\StructTemporaries.cs",
                "a = 1",
                "a = 1"
            );

            Console.WriteLine(generatedJs);
            Assert.IsTrue(generatedJs.Contains("a.Value"));
            Assert.IsTrue(generatedJs.Contains("b.Value"));
        }
    }
}
