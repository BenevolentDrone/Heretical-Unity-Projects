using HereticalSolutions.Entities;

using UImGui.Assets;

namespace HereticalSolutions.Modules.Core_DefaultECS.Unity
{
    [ViewComponent]
    public class CursorSwitcherViewComponent
        : AMonoViewComponent
    {
        public CursorData[] Cursors;
        
        public CursorShapesAsset CursorShapesAsset;

        
        public string PreferredCursor;

        public bool Dirty;
    }
}