using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bird.Core
{
    public enum VFXType
    {
        BlockDestroy_Normal, 
        Explosion,
        Cross,
        Laser,
        LineStrike
    }
    
    public class VFXManager : MonoBehaviour
    {
        [Serializable]
        public struct VFXMapping
        {
            public VFXType type;
            public ParticleSystem prefab;
        }
        
        [Header("VFX Pool Settings")]
        [SerializeField] private List<VFXMapping> vfxMappings;
        [SerializeField] private int initialPoolSize = 5;
        
        private Dictionary<VFXType, Queue<ParticleSystem>> vfxPools = new Dictionary<VFXType, Queue<ParticleSystem>>();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            vfxPools.Clear();
            foreach (var mapping in vfxMappings)
            {
                vfxPools[mapping.type] = new Queue<ParticleSystem>();
                for (int i = 0; i < initialPoolSize; i++)
                {
                    ParticleSystem newVFX = Instantiate(mapping.prefab, transform);
                    newVFX.gameObject.SetActive(false);
                    vfxPools[mapping.type].Enqueue(newVFX);
                }
            }
        }

        /// <summary>
        /// 지정된 위치에서 이펙트를 재생하고, 끝나면 자동으로 풀로 반환합니다
        /// </summary>
        public void PlayVFX(VFXType type, Vector3 position)
        {
            ParticleSystem vfx = GetFromPool(type);

            vfx.transform.position = position;
            vfx.gameObject.SetActive(true);
            vfx.Play(true);

            StartCoroutine(ReturnToPoolAfterPlay(vfx, type));
        }

        private ParticleSystem GetFromPool(VFXType type)
        {
            if (vfxPools.ContainsKey(type) && vfxPools[type].Count > 0)
            {
                return vfxPools[type].Dequeue();
            }
            
            ParticleSystem prefab = vfxMappings.Find(x => x.type == type).prefab;
            ParticleSystem newVFX = Instantiate(prefab, transform);
            return newVFX;
        }

        private IEnumerator ReturnToPoolAfterPlay(ParticleSystem vfx, VFXType type)
        {
            yield return new WaitWhile(() => vfx.IsAlive(true));
            
            vfx.gameObject.SetActive(false);
            
            if (!vfxPools.ContainsKey(type)) vfxPools[type] = new Queue<ParticleSystem>();
            vfxPools[type].Enqueue(vfx);
        }
    }
}
