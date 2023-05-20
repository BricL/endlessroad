using UnityEngine;

namespace Formalin.DRS.Game
{
    public sealed class EndlessRoad : MonoBehaviour
    {
        public float spacing = 0f; // The spacing between the objects
        public float speed = -3.0f;

        private Transform[] _objects;
        private Vector3 _startPos = Vector3.zero;
        private Vector3 _endPos = Vector3.zero;
        private Vector3 _dir = Vector3.zero;
        private float _totalDistance = 0.0f;

        private void Start()
        {
            ArrangeObjects();

            // Setup startPos and endPos
            Bounds bound = GetBounds(_objects[0]);

            _startPos = _objects[0].position;
            _endPos = _objects[_objects.Length - 1].position;
            _dir = (_startPos - _endPos).normalized;
            _totalDistance = Vector3.Distance(_startPos, _endPos);
        }

        private float Dist(Vector3 currentPos, Vector3 targetPos, Vector3 dir)
        {
            Vector3 direction = (targetPos - currentPos).normalized;
            float distance = Vector3.Dot(direction, dir) * Vector3.Distance(targetPos, currentPos);
            return distance;
        }

        private void Update()
        {
            Vector3 movement = _dir * speed * Time.deltaTime;

            // Step1.先移動
            for (var i = 0; i < _objects.Length; i++)
            {
                var obj = _objects[i];
                obj.position = obj.position + movement;
            }

            // Step2.在檢查，確保不會出現"被對齊物件"在對齊後又被移動，導致出現缺口
            for (var i = 0; i < _objects.Length; i++)
            {
                var obj = _objects[i];
                float distanceToStartNEW = Dist(obj.position, _startPos, (_startPos - _endPos).normalized);
                float t = Mathf.Round(distanceToStartNEW / _totalDistance * 1000f) / 1000f;

                if (speed >= 0)
                {
                    if (t <= 0.0f)
                    {
                        var i2Next = 0;
                        if (i == 0)
                            i2Next = _objects.Length - 1;
                        else
                            i2Next = i - 1;

                        Bounds boundObj = GetBounds(_objects[i]);
                        Bounds boundObj2Next = GetBounds(_objects[i2Next]);

                        obj.position = _objects[i2Next].position + new Vector3(0.0f, 0.0f, (boundObj.size.z * 0.5f) + (boundObj2Next.size.z * 0.5f));
                    }
                }
                else
                {
                    if (t >= 1.0f)
                    {
                        var i2Next = 0;
                        if (i == _objects.Length - 1)
                            i2Next = 0;
                        else
                            i2Next = i + 1;

                        Bounds boundObj = GetBounds(_objects[i]);
                        Bounds boundObj2Next = GetBounds(_objects[i2Next]);

                        obj.position = _objects[i2Next].position - new Vector3(0.0f, 0.0f, (boundObj.size.z * 0.5f) + (boundObj2Next.size.z * 0.5f));
                    }
                }
            }
        }

        private void ArrangeObjects()
        {
            // Get the children of this GameObject and sort them by position
            _objects = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                _objects[i] = transform.GetChild(i);
            }

            // Calculate the total depth of the objects with spacing
            float totalDepth = 0f;
            foreach (Transform obj in _objects)
            {
                Bounds bounds = GetBounds(obj);
                totalDepth += bounds.size.z + spacing;
            }

            // Position the first object at the back of the layout
            float currentZ = transform.position.z - totalDepth / 2f;
            foreach (Transform obj in _objects)
            {
                Bounds bounds = GetBounds(obj);
                obj.position = new Vector3(transform.position.x, transform.position.y, currentZ + bounds.size.z / 2f);
                currentZ += bounds.size.z + spacing;
            }

            // Helper method to compare Transforms by their position along the Z-axis
            System.Array.Sort(_objects, (Transform a, Transform b) =>
            {
                return a.position.z.CompareTo(b.position.z);
            });
        }

        // Helper method to get the bounding box of a GameObject, accounting for its children
        private Bounds GetBounds(Transform obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(obj.position, Vector3.zero);
            }
            else if (renderers.Length == 1)
            {
                return renderers[0].bounds;
            }
            else
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }
        }

        private void Reset()
        {
            Start();
            Debug.Log("Reset");
        }
    }

}