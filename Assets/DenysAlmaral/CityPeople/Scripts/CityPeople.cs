using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityPeople
{
    public class CityPeople : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Play one walking animation")]
        private bool PlayWalkAnimation = true;

        [SerializeField]
        [Tooltip("Name of the walking animation state")]
        private string WalkAnimationName = "locom_m_slowWalk_40f";

        [SerializeField]
        [Tooltip("Add a capsule collider to the NPC")]
        private bool AddClickCollider = true;

        [SerializeField]
        [Tooltip("Overrides palette materials, skips other objects")]
        private Material PaletteOverride;

        public string CurrentPaletteName { get; private set; }

        private Animator animator;

        public const string people_pal_prefix = "people_pal";
        private List<Renderer> _paletteMeshes;

        private void Awake()
        {
            Renderer[] allRenderers = gameObject.GetComponentsInChildren<Renderer>();
            _paletteMeshes = new List<Renderer>();

            foreach (Renderer r in allRenderers)
            {
                if (r.sharedMaterial == null)
                {
                    continue;
                }

                string matName = r.sharedMaterial.name;

                if (matName.StartsWith(people_pal_prefix))
                {
                    _paletteMeshes.Add(r);
                }
            }

            if (_paletteMeshes.Count > 0)
            {
                CurrentPaletteName = _paletteMeshes[0].sharedMaterial.name;
            }

            if (PaletteOverride != null)
            {
                SetPalette(PaletteOverride);
            }
        }

        private void Start()
        {
            animator = GetComponent<Animator>();

            if (animator != null && PlayWalkAnimation)
            {
                animator.Play(WalkAnimationName);
            }

            if (AddClickCollider && GetComponent<CapsuleCollider>() == null)
            {
                CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();

                collider.center = new Vector3(0f, 0.8f, 0f);
                collider.radius = 0.3f;
                collider.height = 1.77f;
                collider.direction = 1;
            }
        }

        public void SetPalette(Material mat)
        {
            if (mat == null)
            {
                return;
            }

            if (mat.name.StartsWith(people_pal_prefix))
            {
                CurrentPaletteName = mat.name;

                foreach (Renderer r in _paletteMeshes)
                {
                    r.material = mat;
                }
            }
            else
            {
                Debug.Log("Material name should start with 'people_pal' by convention.");
            }
        }

        public void PlayWalkAnimationNow()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator != null)
            {
                animator.Play(WalkAnimationName);
            }
        }
    }
}