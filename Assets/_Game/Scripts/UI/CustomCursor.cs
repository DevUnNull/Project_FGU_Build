using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.UI
{
    /// <summary>
    /// Quản lý con trỏ chuột tùy chỉnh trong game (Custom Cursor).
    /// Hỗ trợ tự động áp dụng khi vào game, đổi scene và khi chuyển đổi cửa sổ (đặc biệt cho WebGL).
    /// </summary>
    public class CustomCursor : MonoBehaviour
    {
        public static CustomCursor Instance { get; private set; }

        [Header("Cursor Settings")]
        [Tooltip("Texture con trỏ chuột")]
        [SerializeField] private Texture2D cursorTexture;

        [Tooltip("Điểm nhấp chuột (Hotspot) - Tọa độ tính từ góc trên cùng bên trái của texture")]
        [SerializeField] private Vector2 hotspot = new Vector2(2f, 1f);

        [Tooltip("Chế độ con trỏ (Auto: Hardware cursor mượt mà)")]
        [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

        [Tooltip("Tự động áp dụng con trỏ khi game bắt đầu")]
        [SerializeField] private bool applyOnAwake = true;

        [Tooltip("Tự động kích hoạt lại khi người chơi focus lại vào cửa sổ game (hữu ích cho WebGL)")]
        [SerializeField] private bool reapplyOnFocus = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (cursorTexture == null)
            {
                cursorTexture = Resources.Load<Texture2D>("Cursor_64");
            }

            if (applyOnAwake)
            {
                ApplyCursor();
            }
        }

        private void OnEnable()
        {
            ApplyCursor();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && reapplyOnFocus)
            {
                ApplyCursor();
            }
        }

        /// <summary>
        /// Áp dụng con trỏ chuột tùy chỉnh
        /// </summary>
        public void ApplyCursor()
        {
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
            }
        }

        /// <summary>
        /// Đổi con trỏ chuột mới tại runtime
        /// </summary>
        public void SetCursor(Texture2D newCursor, Vector2 newHotspot)
        {
            cursorTexture = newCursor;
            hotspot = newHotspot;
            ApplyCursor();
        }

        /// <summary>
        /// Trả về con trỏ mặc định của hệ thống
        /// </summary>
        public void ResetToDefault()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// Tự động khởi chạy con trỏ khi load game (kể cả khi không có GameObject trong Scene)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            Texture2D tex = Resources.Load<Texture2D>("Cursor_64");
            if (tex != null)
            {
                Cursor.SetCursor(tex, new Vector2(2f, 1f), CursorMode.Auto);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Cursor/Áp Dụng Con Trỏ Mới (Apply Game Cursor)")]
        public static void ApplyCursorInEditor()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/UI/Common/Cursor_64.png");
            if (tex != null)
            {
                PlayerSettings.defaultCursor = tex;
                PlayerSettings.cursorHotspot = new Vector2(2f, 1f);
                AssetDatabase.SaveAssets();
                Cursor.SetCursor(tex, new Vector2(2f, 1f), CursorMode.Auto);
                Debug.Log("<color=green>[CustomCursor]</color> Đã áp dụng con trỏ chuột 'Cursor_64' thành công!");
            }
            else
            {
                Debug.LogError("[CustomCursor] Không tìm thấy file 'Assets/_Game/Art/UI/Common/Cursor_64.png'!");
            }
        }
#endif
    }
}
