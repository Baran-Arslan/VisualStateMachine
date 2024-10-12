using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace iCare.Core.Editor {
    internal sealed class TransitionEdge : Edge {
        const float EdgeOffset = 4;
        const float ArrowWidth = 20;
        const float SelfArrowOffset = 35;

        public TransitionEdge() {
            edgeControl.RegisterCallback<GeometryChangedEvent>(OnEdgeGeometryChanged);
            generateVisualContent += DrawArrow;
        }

        void OnEdgeGeometryChanged(GeometryChangedEvent evt) {
            PointsAndTangents[1] = PointsAndTangents[0];
            PointsAndTangents[2] = PointsAndTangents[3];

            if (input != null && output != null) {
                AddHorizontalOffset();
                AddVerticalOffset();
            }

            MarkDirtyRepaint();
        }

        void AddHorizontalOffset() {
            if (input.node.GetPosition().x > output.node.GetPosition().x) {
                PointsAndTangents[1].y -= EdgeOffset;
                PointsAndTangents[2].y -= EdgeOffset;
            }
            else if (input.node.GetPosition().x < output.node.GetPosition().x) {
                PointsAndTangents[1].y += EdgeOffset;
                PointsAndTangents[2].y += EdgeOffset;
            }
        }

        void AddVerticalOffset() {
            if (input.node.GetPosition().y > output.node.GetPosition().y) {
                PointsAndTangents[1].x += EdgeOffset;
                PointsAndTangents[2].x += EdgeOffset;
            }
            else if (input.node.GetPosition().y < output.node.GetPosition().y) {
                PointsAndTangents[1].x -= EdgeOffset;
                PointsAndTangents[2].x -= EdgeOffset;
            }
        }

        void DrawArrow(MeshGenerationContext context) {
            var start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            var end = PointsAndTangents[PointsAndTangents.Length / 2];
            var mid = (start + end) / 2;
            var direction = end - start;

            if (IsSelfTransition()) {
                mid = PointsAndTangents[0] + Vector2.up * SelfArrowOffset;
                direction = Vector2.down;
            }

            var distanceFromMid = ArrowWidth * Mathf.Sqrt(3) / 4;
            var angle = Vector2.SignedAngle(Vector2.right, direction);
            var perpendicularLength = GetPerpendicularLength(angle);

            var perpendicular = new Vector2(-direction.y, direction.x).normalized * perpendicularLength;

            if (IsSelfTransition()) perpendicular = Vector2.right * perpendicularLength;

            var mesh = context.Allocate(3, 3);
            var vertices = new Vertex[3];

            vertices[0].position = mid + direction.normalized * distanceFromMid;
            vertices[1].position = mid + -direction.normalized * distanceFromMid +
                                   perpendicular.normalized * ArrowWidth / 2;
            vertices[2].position = mid + -direction.normalized * distanceFromMid +
                                   -perpendicular.normalized * ArrowWidth / 2;

            for (var i = 0; i < vertices.Length; i++) {
                vertices[i].position += Vector3.forward * Vertex.nearZ;
                vertices[i].tint = GetColor();
            }

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[] { 0, 1, 2 });
        }

        bool IsSelfTransition() {
            if (input != null && output != null) return input.node == output.node;
            return false;
        }

        static float GetPerpendicularLength(float angle) {
            return angle switch {
                < 60 and > 0 => ArrowWidth / (Mathf.Sin(Mathf.Deg2Rad * (angle + 120)) * 2),
                > -120 and < -60 => ArrowWidth / (Mathf.Sin(Mathf.Deg2Rad * (angle - 120)) * 2),
                > -60 and < 0 => ArrowWidth / (Mathf.Sin(Mathf.Deg2Rad * (angle + 60)) * 2),
                _ => ArrowWidth / (Mathf.Sin(Mathf.Deg2Rad * (angle - 60)) * 2)
            };
        }

        Color GetColor() {
            var color = defaultColor;

            if (output != null) color = new Color(1f, 0.63f, 0.35f);

            if (selected) color = Color.yellow;

            if (isGhostEdge) color = new Color(1f, 0.63f, 0.35f);

            return color;
        }
    }
}