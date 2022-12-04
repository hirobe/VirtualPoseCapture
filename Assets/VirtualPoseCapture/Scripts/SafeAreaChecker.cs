// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace VirtualPoseCapture
{
    // for iPhone Safe Area
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class SafeAreaChecker : MonoBehaviour
    {
        Rect safeArea = new Rect();

        void Update()
        {
            if (safeArea != Screen.safeArea)
            {
                //SaveArea.OnNext(Screen.safeArea);
                safeArea = Screen.safeArea;
                var resolution = new Vector2Int(Screen.width, Screen.height);
                var normalizedMin = new Vector2(safeArea.xMin / resolution.x, safeArea.yMin / resolution.y);
                var normalizedMax = new Vector2(safeArea.xMax / resolution.x, safeArea.yMax / resolution.y);

                var rectTransform = (RectTransform)transform;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchorMin = normalizedMin;
                rectTransform.anchorMax = normalizedMax;
            }
        }
    }
}