﻿using GShark.Core;
using GShark.ExtendedMethods;
using GShark.Interfaces;
using GShark.Operation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GShark.Geometry
{
    /// <summary>
    /// Represents a curve that is the result of joining several (possibly different) types of curves.
    /// </summary>
    /// <example>
    /// [!code-csharp[Example](../../src/GShark.Test.XUnit/Geometry/PolyCurveTests.cs?name=example)]
    /// </example>
    public class PolyCurve : NurbsBase
    {
        private readonly List<ICurve> _segments = new List<ICurve>();
        // ToDo: this list will be removed after all the other curve will inheritance from NurbsBase.
        private readonly List<NurbsBase> _segmentsNurbs = new List<NurbsBase>();

        /// <summary>
        /// Initializes a new empty polyCurve.
        /// </summary>
        public PolyCurve()
        { }

        /// <summary>
        /// Appends and matches the start of the arc to the end of polycurve.
        /// This function will fail if the polycurve is closed.
        /// </summary>
        /// <param name="line">The line to append.</param>
        public void Append(Line line)
        {
            HealthChecks(line);
            _segments.Add(line);
            _segmentsNurbs.Add(line.ToNurbs());
            ToNurbsForm();
        }

        /// <summary>
        /// Appends and matches the start of the arc to the end of polycurve.
        /// This function will fail if the polycurve is closed.
        /// </summary>
        /// <param name="arc">The arc to append.</param>
        public void Append(Arc arc)
        {
            HealthChecks(arc);
            _segments.Add(arc);
            _segmentsNurbs.Add(arc.ToNurbs());
            ToNurbsForm();
        }

        /// <summary>
        /// Appends and matches the start of the curve to the end of polycurve.
        /// This function will fail if the polycurve is closed or if SegmentCount > 0 and the segment curve is closed.
        /// </summary>
        /// <param name="curve"></param>
        public void Append(NurbsCurve curve)
        {
            if (curve.IsClosed())
            {
                throw new InvalidOperationException("The curve is closed.");
            }
            HealthChecks(curve);
            _segments.Add(curve);
            _segmentsNurbs.Add(curve);
            ToNurbsForm();
        }

        /// <summary>
        /// Appends and matches the start of the curve to the end of polycurve.
        /// This function will fail if the polycurve is closed or if SegmentCount > 0 and the segment polycurve is closed.
        /// </summary>
        /// <param name="polyCurve"></param>
        public void Append(PolyCurve polyCurve)
        {
            if (polyCurve.IsClosed())
            {
                throw new InvalidOperationException("The polycurve is closed.");
            }
            HealthChecks(polyCurve);
            _segments.Add(polyCurve);
            _segmentsNurbs.Add(polyCurve);
            ToNurbsForm();
        }

        /// <summary>
        /// Internal initialization of polycurve.
        /// No health checks are made, you are responsible of the collection of curve you are passing.
        /// </summary>
        /// <param name="curves"></param>
        internal void Append(List<NurbsBase> curves)
        {
            _segments.AddRange(curves);
            _segmentsNurbs.AddRange(curves);
            ToNurbsForm();
        }

        /// <summary>
        /// The segments of the polyCurve.
        /// </summary>
        public List<ICurve> Segments => _segments;

        /// <summary>
        /// Defines the NURBS form of the polyline.
        /// </summary>
        private void ToNurbsForm()
        {
            if (_segments.Count == 1)
            {
                Weights = Point4.GetWeights(_segmentsNurbs[0].ControlPoints);
                Degree = _segmentsNurbs[0].Degree;
                Knots = _segmentsNurbs[0].Knots;
                ControlPointLocations = Point4.PointDehomogenizer1d(_segmentsNurbs[0].ControlPoints);
                ControlPoints = _segmentsNurbs[0].ControlPoints;
                return;
            }

            // Extract the biggest degree between the curves.
            int finalDegree = _segmentsNurbs.Max(c => c.Degree);

            // Homogenized degree curves.
            IEnumerable<NurbsBase> homogenizedCurves = _segmentsNurbs.Select(curve => curve.Degree != finalDegree ? Modify.ElevateDegree(curve, finalDegree) : curve);

            // Join curves.
            List<double> joinedKnots = new List<double>();
            List<Point4> joinedControlPts = new List<Point4>();

            joinedKnots.AddRange(homogenizedCurves.First().Knots.Take(homogenizedCurves.First().Knots.Count - 1));
            joinedControlPts.AddRange(homogenizedCurves.First().ControlPoints);

            foreach (NurbsBase curve in homogenizedCurves.Skip(1))
            {
                joinedKnots.AddRange(curve.Knots.Take(curve.Knots.Count - 1).Skip(finalDegree + 1).Select(k => k + joinedKnots.Last()).ToList());
                joinedControlPts.AddRange(curve.ControlPoints.Skip(1));
            }

            // Appending the last knot to the end.
            joinedKnots.Add(joinedKnots.Last());

            Weights = Point4.GetWeights(joinedControlPts);
            Degree = finalDegree;
            Knots = joinedKnots.ToKnot().Normalize();
            ControlPointLocations = Point4.PointDehomogenizer1d(joinedControlPts);
            ControlPoints = joinedControlPts;
        }

        /// <summary>
        /// Checks to define if the curve can be appended to the polycurve.
        /// </summary>
        private void HealthChecks(ICurve curve)
        {
            if (_segments.Count <= 0) return;

            if (IsClosed())
            {
                throw new InvalidOperationException($"The polyCurve is closed can not be possible to connect the {curve.GetType()}.");
            }

            if (EndPoint.DistanceTo(curve.StartPoint) > GSharkMath.Epsilon)
            {
                throw new InvalidOperationException("The two curves can not be connected.");
            }
        }
    }
}
