using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Tilemaps
{
    internal class GridPaintPaletteWindowSplitView : VisualElement
    {
        private static readonly string ussClassName = "unity-tilepalette-splitview";
        private static readonly string splitViewDataKey = "unity-tilepalette-splitview-data";

        private const float kMinSplitRatio = 0.3f;

        private TwoPaneSplitView m_SplitView;
        private TilePaletteElement m_PaletteElement;
        private TilePaletteBrushElementToggle m_BrushElementToggle;
        private float m_LastSplitRatio = kMinSplitRatio;

        public TilePaletteElement paletteElement => m_PaletteElement;

#if DISABLE_BRUSH_EDITOR
        public bool isVerticalOrientation => true;
#else   
        public bool isVerticalOrientation
        {
            get
            {
                return m_SplitView.orientation == TwoPaneSplitViewOrientation.Vertical;
            }
            set
            {
                m_SplitView.orientation =
                    value ? TwoPaneSplitViewOrientation.Vertical : TwoPaneSplitViewOrientation.Horizontal;
            }
        }
#endif

        private float fullLength => isVerticalOrientation ? layout.height : layout.width;

#if DISABLE_BRUSH_EDITOR
        private bool isMinSplit => false;
#else
        private bool isMinSplit => (fullLength - m_SplitView.fixedPaneDimension) <= minBottomSplitDimension;
#endif
        
        private float minTopSplitDimension => isVerticalOrientation ? 24f : 12f;
        private float minBottomSplitDimension => isVerticalOrientation ? 24f : 12f;

        public void ChangeSplitDimensions(float dimension)
        {
#if !DISABLE_BRUSH_EDITOR
            var newLength = fullLength - dimension;
            var diff = newLength - m_SplitView.fixedPaneDimension;
            if (m_SplitView.m_Resizer != null)
                m_SplitView.m_Resizer.ApplyDelta(diff);
#endif
        }

        public GridPaintPaletteWindowSplitView(bool isVerticalOrientation)
        {
            AddToClassList(ussClassName);

            name = "tilePaletteSplitView";
            TilePaletteOverlayUtility.SetStyleSheet(this);

            m_PaletteElement = new TilePaletteElement();

#if DISABLE_BRUSH_EDITOR
            Add(m_PaletteElement);
#else
            var brushesElement = new TilePaletteBrushModalElement();
            m_SplitView = new TwoPaneSplitView(0, -1, isVerticalOrientation ? TwoPaneSplitViewOrientation.Vertical : TwoPaneSplitViewOrientation.Horizontal);
            m_SplitView.contentContainer.Add(m_PaletteElement);
            m_SplitView.contentContainer.Add(brushesElement);
            Add(m_SplitView);

            m_SplitView.viewDataKey = splitViewDataKey;

            brushesElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            m_BrushElementToggle = this.Q<TilePaletteBrushElementToggle>();
            m_BrushElementToggle.ToggleChanged += BrushElementToggleChanged;
            m_BrushElementToggle.SetValueWithoutNotify(!isMinSplit);
#endif
        }

        private void BrushElementToggleChanged(bool show)
        {
            var dimension = minBottomSplitDimension;
            if (show)
            {
                dimension = m_LastSplitRatio * fullLength;
                if (dimension < minBottomSplitDimension)
                    dimension = kMinSplitRatio * fullLength;
            }
            ChangeSplitDimensions(dimension);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
#if !DISABLE_BRUSH_EDITOR
            m_BrushElementToggle.SetValueWithoutNotify(!isMinSplit);

            if (m_SplitView.fixedPaneDimension < 0f)
            {
                var defaultLength = fullLength * (1.0f - kMinSplitRatio);
                m_SplitView.fixedPaneInitialDimension = defaultLength;
                ChangeSplitDimensions(defaultLength);
            }

            var newDimension = fullLength - m_SplitView.fixedPaneDimension;
            if (fullLength > minBottomSplitDimension)
            {
                // Force the palette toolbar to always be shown
                if (m_SplitView.fixedPaneDimension < minTopSplitDimension)
                {
                    ChangeSplitDimensions(fullLength - minTopSplitDimension);
                }
                // Force the brush toolbar to always be shown
                if (newDimension < minBottomSplitDimension)
                {
                    ChangeSplitDimensions(minBottomSplitDimension);
                }
            }
            if (newDimension > minBottomSplitDimension)
            {
                var newLastSplit = Mathf.Max(newDimension, kMinSplitRatio * fullLength);
                m_LastSplitRatio = newLastSplit / fullLength;
            }
#endif
        }
    }
}
