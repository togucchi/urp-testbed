using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Toguchi.Rendering
{
    public class BoidsRenderer : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private int instanceCount = 1000;
        private BoidsRenderData _renderData;
        
        private void OnEnable()
        {
            if (_mesh == null || _material == null)
            {
                return;
            }

            _renderData = new BoidsRenderData(_mesh, _material, (uint) instanceCount);
            BoidsPass.RenderDataMap.Add(this.GetInstanceID(), _renderData);
        }

        private void OnDisable()
        {
            if (_renderData == null)
            {
                return;
            }

            BoidsPass.RenderDataMap.Remove(this.GetInstanceID());
            _renderData?.Dispose();
            _renderData = null;
        }

        [BurstCompile]
        private struct BoidsUpdateJob : IJobParallelFor
        {
            public void Execute(int index)
            {
                
            }

            private void UpdateWall(int index)
            {
                
            }
        }
    }

    public class BoidsRenderData : IDisposable
    {
        public Mesh Mesh { get; }
        public Material Material { get; }
        public uint InstanceCount { get; }

        public GraphicsBuffer ArgsBuffer { get; }
        public GraphicsBuffer DataBuffer { get; }
        
        private uint[] args = new uint[5];
        
        public BoidsRenderData(Mesh mesh, Material material, uint instanceCount)
        {
            this.Mesh = mesh;
            this.Material = material;
            this.InstanceCount = instanceCount;

            ArgsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, args.Length * sizeof(uint));
            DataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)instanceCount, Marshal.SizeOf(typeof(BufferData)));
            
            UpdateBuffer();
            CreatePosBuffer();
        }

        public void Dispose()
        {
            ArgsBuffer?.Dispose();
            DataBuffer?.Dispose();
        }
        
        private void CreatePosBuffer()
        {
            if (Mesh == null || Material == null)
            {
                return;
            }
            var buffers = new BufferData[InstanceCount];

            for (int i = 0; i < InstanceCount; ++i)
            {
                var position = new float4(Random.Range(-100, 100f), Random.Range(-100, 100f), Random.Range(-100, 100f),
                    Random.Range(0.1f, 1f));

                var color = new float4(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);

                buffers[i].position = position;
                buffers[i].color= color;
            }

            DataBuffer.SetData(buffers);
            Material.SetBuffer("_DataBuffer", DataBuffer);
        }

        private void UpdateBuffer()
        {
            if (Mesh == null || Material == null)
            {
                return;
            }

            args[0] = Mesh.GetIndexCount(0);
            args[1] = InstanceCount;
            args[2] = Mesh.GetIndexStart(0);
            args[3] = Mesh.GetBaseVertex(0);

            ArgsBuffer.SetData(args);
        }

        public struct BufferData
        {
            public float4 position;
            public float4 color;
        }
    }
}