using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace RemoveBackgroundVideo.EditorTools
{
    public class VideoChromaKeyTool : EditorWindow
    {
        private GameObject targetObject;
        private VideoClip videoClip;
        private List<Color> keyColors = new List<Color>() { Color.white };
        private float threshold = 0.5f;
        private float softness = 0.2f;

        [MenuItem("Tools/Remove Background Video/Open Tool Window")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(VideoChromaKeyTool), false, "Video Chroma Key");
            window.minSize = new Vector2(350, 450);
        }

        private void OnEnable()
        {
            if (Selection.activeGameObject != null)
            {
                targetObject = Selection.activeGameObject;
                LoadSettingsFromMaterial();
            }
        }

        private void LoadSettingsFromMaterial()
        {
            if (targetObject == null) return;
            RawImage rawImg = targetObject.GetComponent<RawImage>();
            if (rawImg != null && rawImg.material != null && rawImg.material.shader.name == "Custom/VideoChromaKey")
            {
                Material mat = rawImg.material;
                if (mat.HasProperty("_Threshold")) threshold = mat.GetFloat("_Threshold");
                if (mat.HasProperty("_Softness")) softness = mat.GetFloat("_Softness");
                
                if (mat.HasProperty("_KeyColor1"))
                {
                    keyColors.Clear();
                    for (int i = 1; i <= 5; i++)
                    {
                        string prop = "_KeyColor" + i;
                        if (mat.HasProperty(prop))
                        {
                            Color c = mat.GetColor(prop);
                            if (c.a > 0) keyColors.Add(c);
                        }
                    }
                    if (keyColors.Count == 0) keyColors.Add(Color.white); // Default
                }
            }
            
            VideoPlayer vp = targetObject.GetComponent<VideoPlayer>();
            if (vp != null) videoClip = (VideoClip)vp.clip;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("🔧 CẤU HÌNH TOOL XÓA NỀN VIDEO", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Kéo thả Object (Logo/Image) từ Hierarchy vào ô bên dưới, sau đó thiết lập thông số và bấm Apply.", MessageType.Info);
            
            GUILayout.Space(10);
            
            EditorGUI.BeginChangeCheck();
            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object (UI)", targetObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                LoadSettingsFromMaterial();
            }
            
            videoClip = (VideoClip)EditorGUILayout.ObjectField("Video Clip", videoClip, typeof(VideoClip), false);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Chroma Key Settings (Xóa nền)", EditorStyles.boldLabel);
            
            // Render list of colors
            for (int i = 0; i < keyColors.Count; i++)
            {
                GUILayout.BeginHorizontal();
                keyColors[i] = EditorGUILayout.ColorField(new GUIContent("Color " + (i + 1)), keyColors[i], true, false, false);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    keyColors.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
            
            if (keyColors.Count < 5)
            {
                if (GUILayout.Button("+ Thêm màu (Max 5)", GUILayout.Height(25)))
                {
                    keyColors.Add(Color.white);
                }
            }

            GUILayout.Space(10);
            threshold = EditorGUILayout.Slider("Threshold (Ngưỡng xóa)", threshold, 0f, 1f);
            softness = EditorGUILayout.Slider("Softness (Độ mềm viền)", softness, 0f, 1f);

            GUILayout.Space(20);

            if (GUILayout.Button("🚀 APPLY (Thực hiện)", GUILayout.Height(40)))
            {
                ApplyChromaKey();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Khôi phục lại thành Image (Undo)", GUILayout.Height(25)))
            {
                RevertToImage();
            }
        }

        private void ApplyChromaKey()
        {
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng kéo 1 Object vào ô Target Object!", "OK");
                return;
            }

            Image img = targetObject.GetComponent<Image>();
            if (img != null) DestroyImmediate(img);

            RawImage rawImg = targetObject.GetComponent<RawImage>();
            if (rawImg == null) rawImg = targetObject.AddComponent<RawImage>();
            
            VideoPlayer vp = targetObject.GetComponent<VideoPlayer>();
            if (vp == null) vp = targetObject.AddComponent<VideoPlayer>();
            
            vp.renderMode = VideoRenderMode.APIOnly;
            vp.playOnAwake = true;
            vp.isLooping = true;
            if (videoClip != null) vp.clip = videoClip;
            
            Shader chromaShader = Shader.Find("Custom/VideoChromaKey");
            if (chromaShader != null)
            {
                string matDir = "Assets/Remove_Background_Video/Materials";
                string matPath = matDir + "/MAT_VideoChromaKey.mat";
                
                if (!System.IO.Directory.Exists(matDir))
                {
                    System.IO.Directory.CreateDirectory(matDir);
                    AssetDatabase.Refresh();
                }

                Material chromaMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (chromaMat == null)
                {
                    chromaMat = new Material(chromaShader);
                    AssetDatabase.CreateAsset(chromaMat, matPath);
                }
                
                // Set Key Colors explicitly for serialization
                for (int i = 0; i < 5; i++)
                {
                    string prop = "_KeyColor" + (i + 1);
                    if (i < keyColors.Count)
                    {
                        Color c = keyColors[i];
                        c.a = 1f; // Đảm bảo alpha > 0 để shader nhận diện
                        chromaMat.SetColor(prop, c);
                    }
                    else
                    {
                        chromaMat.SetColor(prop, new Color(0, 0, 0, 0)); // Alpha 0 báo cho shader bỏ qua
                    }
                }

                chromaMat.SetFloat("_Threshold", threshold);
                chromaMat.SetFloat("_Softness", softness);

                rawImg.material = chromaMat;
                EditorUtility.SetDirty(chromaMat);
            }
            else
            {
                Debug.LogWarning("Chưa tìm thấy Shader Custom/VideoChromaKey.");
            }

            var scriptType = typeof(RemoveBackgroundVideo.UIVideoPlayer);
            if (targetObject.GetComponent(scriptType) == null)
            {
                targetObject.AddComponent(scriptType);
            }

            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            EditorUtility.DisplayDialog("Thành công", "Đã Setup xong Video Chroma Key đa màu sắc!", "OK");
        }

        private void RevertToImage()
        {
            if (targetObject == null) return;
            
            var scriptType = typeof(RemoveBackgroundVideo.UIVideoPlayer);
            var comp = targetObject.GetComponent(scriptType);
            if (comp != null) DestroyImmediate(comp);
            
            var vp = targetObject.GetComponent<VideoPlayer>();
            if (vp != null) DestroyImmediate(vp);
            
            var rawImg = targetObject.GetComponent<RawImage>();
            if (rawImg != null) DestroyImmediate(rawImg);
            
            if (targetObject.GetComponent<Image>() == null)
            {
                targetObject.AddComponent<Image>();
            }
            
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            EditorUtility.DisplayDialog("Undo", "Đã khôi phục lại thành Image thông thường.", "OK");
        }
    }
}
