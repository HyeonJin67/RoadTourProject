using System;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools;

namespace XDPaint.Core.PaintObject.States
{
    public class TexturesKeeper
    {
        public Action OnChangeState;
        public Action OnResetState;
        public Action<RenderTexture> OnMouseUp;
        public Action OnReDraw;
        public Action OnUndo;
        public Action OnRedo;
        public event Action<bool> OnUndoStatusChanged;
        public event Action<bool> OnRedoStatusChanged;

        private readonly List<RenderTexture> textures = new List<RenderTexture>();
        private Action<RenderTexture> onRender;
        private int currentStateIndex;
        private bool isEnabled;
        private bool lockOnFirstTexture;

        public void Init(Action<RenderTexture> renderAction, bool enabled)
        {
            isEnabled = enabled;
            onRender = renderAction;
            OnMouseUp = texture =>
            {
                if (!isEnabled)
                    return;
                
                if (textures.Count > 0)
                {
                    for (var i = textures.Count - 1; i >= currentStateIndex; i--)
                    {
                        var element = textures[i];
                        RenderTexture.ReleaseTemporary(element);
                        textures.RemoveAt(i);
                    }

                    lockOnFirstTexture = currentStateIndex + 1 > Settings.Instance.UndoRedoMaxActionsCount;
                    if (lockOnFirstTexture && textures.Count >= Settings.Instance.UndoRedoMaxActionsCount + 1)
                    {
                        RenderTexture.ReleaseTemporary(textures[0]);
                        textures.RemoveAt(0);
                    }
                }
                textures.Add(texture);
                currentStateIndex = textures.Count;
                UpdateStatus();
            };
        }

        /// <summary>
        /// Undo
        /// </summary>
        public void Undo()
        {
            if (!isEnabled)
                return;

            var newIndex = currentStateIndex - 2;
            var lockedTextureIndex = lockOnFirstTexture ? 0 : 1;
            if (newIndex + lockedTextureIndex >= 0)
            {
                OnResetState();
            }
            OnChangeState();
            OnReDraw = () =>
            {
                RenderTexture texture = null;
                if (newIndex >= 0)
                {
                    texture = textures[newIndex];
                }
                onRender(texture);
            };
            currentStateIndex--;
            if (currentStateIndex < 0)
            {
                currentStateIndex = 0;
            }
            
            UpdateStatus();

            if (OnUndo != null)
            {
                OnUndo();
            }
        }

        /// <summary>
        /// Redo
        /// </summary>
        public void Redo()
        {
            if (!isEnabled)
                return;
            
            OnChangeState();
            currentStateIndex++;
            var newIndex = currentStateIndex - 1;
            if (newIndex >= 0)
            {
                OnReDraw = () => onRender(textures[newIndex]);
            }
            if (currentStateIndex > textures.Count)
            {
                currentStateIndex = textures.Count;
            }

            UpdateStatus();
            
            if (OnRedo != null)
            {
                OnRedo();
            }
        }

        private void UpdateStatus()
        {
            if (OnUndoStatusChanged != null)
            {
                OnUndoStatusChanged(CanUndo());
            }

            if (OnRedoStatusChanged != null)
            {
                OnRedoStatusChanged(CanRedo());
            }
        }

        /// <summary>
        /// Removes all saved Textures from TexturesKeeper
        public void Reset()
        {
            if (!isEnabled)
                return;
            
            currentStateIndex = 0;
            foreach (var state in textures)
            {
                RenderTexture.ReleaseTemporary(state);
                state.DiscardContents();
            }
            textures.Clear();
        }

        /// <summary>
        /// Returns if can Undo
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            var minimalIndex = lockOnFirstTexture ? 1 : 0;
            return textures.Count > 0 && currentStateIndex > minimalIndex;
        }

        /// <summary>
        /// Returns if can Redo
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return textures.Count > 0 && currentStateIndex < textures.Count;
        }
    }
}