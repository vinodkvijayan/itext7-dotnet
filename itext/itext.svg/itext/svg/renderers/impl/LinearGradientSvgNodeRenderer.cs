using System;
using System.Collections.Generic;
using iText.Kernel.Colors;
using iText.Kernel.Colors.Gradients;
using iText.Kernel.Geom;
using iText.Layout.Properties;
using iText.StyledXmlParser.Css.Util;
using iText.Svg;
using iText.Svg.Renderers;

namespace iText.Svg.Renderers.Impl {
    /// <summary>
    /// <see cref="iText.Svg.Renderers.ISvgNodeRenderer"/>
    /// implementation for the &lt;linearGradient&gt; tag.
    /// </summary>
    public class LinearGradientSvgNodeRenderer : AbstractGradientSvgNodeRenderer {
        public override Color CreateColor(SvgDrawContext context, Rectangle objectBoundingBox, float objectBoundingBoxMargin
            , float parentOpacity) {
            if (objectBoundingBox == null) {
                return null;
            }
            LinearGradientBuilder builder = new LinearGradientBuilder();
            foreach (GradientColorStop stopColor in ParseStops(parentOpacity)) {
                builder.AddColorStop(stopColor);
            }
            builder.SetSpreadMethod(ParseSpreadMethod());
            bool isObjectBoundingBox = IsObjectBoundingBoxUnits();
            Point[] coordinates = GetCoordinates(context, isObjectBoundingBox);
            builder.SetGradientVector(coordinates[0].GetX(), coordinates[0].GetY(), coordinates[1].GetX(), coordinates
                [1].GetY());
            AffineTransform gradientTransform = GetGradientTransformToUserSpaceOnUse(objectBoundingBox, isObjectBoundingBox
                );
            builder.SetCurrentSpaceToGradientVectorSpaceTransformation(gradientTransform);
            return builder.BuildColor(objectBoundingBox.ApplyMargins(objectBoundingBoxMargin, objectBoundingBoxMargin, 
                objectBoundingBoxMargin, objectBoundingBoxMargin, true), context.GetCurrentCanvasTransform());
        }

        public override ISvgNodeRenderer CreateDeepCopy() {
            LinearGradientSvgNodeRenderer copy = new LinearGradientSvgNodeRenderer();
            DeepCopyAttributesAndStyles(copy);
            DeepCopyChildren(copy);
            return copy;
        }

        // TODO: DEVSIX-4136 opacity is not supported now.
        //  The opacity should be equal to 'parentOpacity * stopRenderer.getStopOpacity() * stopColor[3]'
        private IList<GradientColorStop> ParseStops(float parentOpacity) {
            IList<GradientColorStop> stopsList = new List<GradientColorStop>();
            foreach (StopSvgNodeRenderer stopRenderer in GetChildStopRenderers()) {
                float[] stopColor = stopRenderer.GetStopColor();
                double offset = stopRenderer.GetOffset();
                stopsList.Add(new GradientColorStop(stopColor, offset, GradientColorStop.OffsetType.RELATIVE));
            }
            if (!stopsList.IsEmpty()) {
                GradientColorStop firstStop = stopsList[0];
                if (firstStop.GetOffset() > 0) {
                    stopsList.Add(0, new GradientColorStop(firstStop, 0f, GradientColorStop.OffsetType.RELATIVE));
                }
                GradientColorStop lastStop = stopsList[stopsList.Count - 1];
                if (lastStop.GetOffset() < 1) {
                    stopsList.Add(new GradientColorStop(lastStop, 1f, GradientColorStop.OffsetType.RELATIVE));
                }
            }
            return stopsList;
        }

        private AffineTransform GetGradientTransformToUserSpaceOnUse(Rectangle objectBoundingBox, bool isObjectBoundingBox
            ) {
            AffineTransform gradientTransform = new AffineTransform();
            if (isObjectBoundingBox) {
                gradientTransform.Translate(objectBoundingBox.GetX(), objectBoundingBox.GetY());
                // We need to scale with dividing the lengths by 0.75 as further we should
                // concatenate gradient transformation matrix which has no absolute parsing.
                // For example, if gradientTransform is set to translate(1, 1) and gradientUnits
                // is set to "objectBoundingBox" then the gradient should be shifted horizontally
                // and vertically exactly by the size of the element bounding box. So, again,
                // as we parse translate(1, 1) to translation(0.75, 0.75) the bounding box in
                // the gradient vector space should be 0.75x0.75 in order for such translation
                // to shift by the complete size of bounding box.
                gradientTransform.Scale(objectBoundingBox.GetWidth() / 0.75, objectBoundingBox.GetHeight() / 0.75);
            }
            AffineTransform svgGradientTransformation = GetGradientTransform();
            if (svgGradientTransformation != null) {
                gradientTransform.Concatenate(svgGradientTransformation);
            }
            return gradientTransform;
        }

        private Point[] GetCoordinates(SvgDrawContext context, bool isObjectBoundingBox) {
            Point start;
            Point end;
            if (isObjectBoundingBox) {
                start = new Point(GetCoordinateForObjectBoundingBox(SvgConstants.Attributes.X1, 0), GetCoordinateForObjectBoundingBox
                    (SvgConstants.Attributes.Y1, 0));
                end = new Point(GetCoordinateForObjectBoundingBox(SvgConstants.Attributes.X2, 1), GetCoordinateForObjectBoundingBox
                    (SvgConstants.Attributes.Y2, 0));
            }
            else {
                Rectangle currentViewPort = context.GetCurrentViewPort();
                double x = currentViewPort.GetX();
                double y = currentViewPort.GetY();
                double width = currentViewPort.GetWidth();
                double height = currentViewPort.GetHeight();
                start = new Point(GetCoordinateForUserSpaceOnUse(SvgConstants.Attributes.X1, x, x, width), GetCoordinateForUserSpaceOnUse
                    (SvgConstants.Attributes.Y1, y, y, height));
                end = new Point(GetCoordinateForUserSpaceOnUse(SvgConstants.Attributes.X2, x + width, x, width), GetCoordinateForUserSpaceOnUse
                    (SvgConstants.Attributes.Y2, y, y, height));
            }
            return new Point[] { start, end };
        }

        private double GetCoordinateForObjectBoundingBox(String attributeName, double defaultValue) {
            String attributeValue = GetAttribute(attributeName);
            double absoluteValue = defaultValue;
            if (CssUtils.IsPercentageValue(attributeValue)) {
                absoluteValue = CssUtils.ParseRelativeValue(attributeValue, 1);
            }
            else {
                if (CssUtils.IsNumericValue(attributeValue) || CssUtils.IsMetricValue(attributeValue) || CssUtils.IsRelativeValue
                    (attributeValue)) {
                    // if there is incorrect value metric, then we do not need to parse the value
                    int unitsPosition = CssUtils.DeterminePositionBetweenValueAndUnit(attributeValue);
                    if (unitsPosition > 0) {
                        // We want to ignore the unit type. From the svg specification:
                        // "the normal of the linear gradient is perpendicular to the gradient vector in
                        // object bounding box space (i.e., the abstract coordinate system where (0,0)
                        // is at the top/left of the object bounding box and (1,1) is at the bottom/right
                        // of the object bounding box)".
                        // Different browsers treats this differently. We chose the "Google Chrome" approach
                        // which treats the "abstract coordinate system" in the coordinate metric measure,
                        // i.e. for value '0.5cm' the top/left of the object bounding box would be (1cm, 1cm),
                        // for value '0.5em' the top/left of the object bounding box would be (1em, 1em) and etc.
                        // no null pointer should be thrown as determine
                        absoluteValue = CssUtils.ParseDouble(attributeValue.JSubstring(0, unitsPosition)).Value;
                    }
                }
            }
            // need to multiply by 0.75 as further the (top, right) coordinates of the object bbox
            // would be transformed into (0.75, 0.75) point instead of (1, 1). The reason described
            // as a comment inside the method constructing the gradient transformation
            return absoluteValue * 0.75;
        }

        private double GetCoordinateForUserSpaceOnUse(String attributeName, double defaultValue, double start, double
             length) {
            String attributeValue = GetAttribute(attributeName);
            double absoluteValue;
            // TODO: DEVSIX-4018 em and rem actual values are obtained. Default is in use
            //  do not forget to add the test to cover these values change
            UnitValue unitValue = CssUtils.ParseLengthValueToPt(attributeValue, 12, 12);
            if (unitValue == null) {
                absoluteValue = defaultValue;
            }
            else {
                if (unitValue.GetUnitType() == UnitValue.PERCENT) {
                    absoluteValue = start + (length * unitValue.GetValue() / 100);
                }
                else {
                    absoluteValue = unitValue.GetValue();
                }
            }
            return absoluteValue;
        }
    }
}