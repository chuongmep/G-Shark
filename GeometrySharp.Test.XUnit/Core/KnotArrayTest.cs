﻿using System;
using FluentAssertions;
using GeometrySharp.Core;
using Xunit;
using Xunit.Abstractions;

namespace GeometrySharp.XUnit.Core
{
    [Trait("Category", "Knot")]
    public class KnotArrayTest
    {
        private readonly ITestOutputHelper _testOutput;

        public KnotArrayTest(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Theory]
        [InlineData(0,12)]
        [InlineData(4,0)]
        [InlineData(-1, 0)]
        public void KnotArray_ThrowsAnException_IfDegreeOrNumberOfControlPts_AreLessThanOrZero(int degree, int numberOfControlPts)
        {
            Func<Knot> funcResult = () => new Knot(degree, numberOfControlPts);

            funcResult.Should().Throw<Exception>().WithMessage("Input values must be positive and different than zero.");
        }

        [Fact]
        public void GenerateAnEqualSpaceKnotArray()
        {
            var degree = 4;
            var ctrlPts = 12;
            var resultExpected = new Knot(){ 0.0, 0.0, 0.0, 0.0, 0.0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875, 1.0, 1.0, 1.0, 1.0, 1.0};

            var generateKnots = new Knot(degree, ctrlPts);

            generateKnots.Should().BeEquivalentTo(resultExpected);
        }

        [Fact]
        public void GenerateAnEqualSpaceKnotArray_Unclamped()
        {
            var degree = 3;
            var ctrlPts = 5;
            var resultExpected = new Knot() { 0.0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875, 1.0 };

            var generateKnots = new Knot(degree, ctrlPts, false);

            generateKnots.Should().BeEquivalentTo(resultExpected);
        }

        [Theory]
        [InlineData(new double[] {0, 0, 1, 2, 3, 4, 4}, 4, 12, false)]
        [InlineData(new double[] {5, 3, 6, 5, 4, 5, 6}, 3, 3, false)]
        [InlineData(new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875, 1.0, 1.0, 1.0, 1.0, 1.0 }, 4, 12, true)]
        public void AreValidKnots(double[] knots, int degree, int ctrlPts, bool expectedResult)
        {
            var knotsArray = new Knot(knots);

            knotsArray.AreValid(degree, ctrlPts).Should().Be(expectedResult);
        }

        [Fact]
        public void NormalizedKnotArray()
        {
            var knots = new Knot(){ -5, -5, -3, -2, 2, 3, 5, 5 };
            var knotsExpected = new Knot(){ 0.0, 0.0, 0.2, 0.3, 0.7, 0.8, 1.0, 1.0 };

            knots.Normalize();

            knots.Should().BeEquivalentTo(knotsExpected);
        }
    }
}