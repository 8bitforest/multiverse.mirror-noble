// using System;
// using System.Diagnostics;
// using System.Linq;
// using Mirror;
// using Multiverse.Serialization;
// using NUnit.Framework;
// using UnityEngine;
// using Debug = UnityEngine.Debug;
//
// namespace Multiverse.MirrorNoble.Tests
// {
//     [TestFixture]
//     public class SerializationTiming
//     {
//         private MvBinaryWriter _mvWriter;
//         private NetworkWriter _mirrorWriter;
//     
//         private const int ReadWrites = 100000;
//         private const int Samples = 100;
//     
//         [SetUp]
//         public void SetUp()
//         {
//             _mvWriter = new MvBinaryWriter();
//             _mirrorWriter = new NetworkWriter();
//         }
//     
//         [Test]
//         public void ReadWriteInt()
//         {
//             var ints = Enumerable.Range(0, ReadWrites).ToArray();
//             Time(i => ints[i]);
//         }
//         
//         [Test]
//         public void ReadWriteMessage()
//         {
//             var message = new TestMessage
//             {
//                 Int = 100,
//                 Long = long.MaxValue,
//                 Vec2 = new Vector2(2000, 3000)
//             };
//             Time(i => message);
//         }
//     
//         private void Time<T>(Func<int, T> getValue)
//         {
//             TimeMv(getValue);
//             TimeMirror(getValue);
//         }
//     
//         private void TimeMv<T>(Func<int, T> getValue)
//         {
//             Time("Multiverse", getValue, w => new MvBinaryReader(w.GetData()), _mvWriter,
//                 r => r.Read<T>(), v => _mvWriter.Write(v));
//         }
//     
//         private void TimeMirror<T>(Func<int, T> getValue)
//         {
//             Time("Mirror", getValue, w => new NetworkReader(w.ToArraySegment()), _mirrorWriter,
//                 r => r.Read<T>(), v => _mirrorWriter.Write(v));
//         }
//         
//         [Test]
//         public void TimeMv()
//         {
//             long totalMilliseconds = 0;
//             var message = new TestMessage
//             {
//                 Int = 100,
//                 Long = long.MaxValue,
//                 Vec2 = new Vector2(2000, 3000)
//             };
//     
//             for (var i = 0; i < Samples; i++)
//             {
//                 var stopwatch = new Stopwatch();
//                 stopwatch.Start();
//                 for (var j = 0; j < ReadWrites; j++)
//                     _mvWriter.Write(message);
//     
//                 stopwatch.Stop();
//                 _mvWriter.Reset();
//                 _mirrorWriter.Reset();
//                 totalMilliseconds += stopwatch.ElapsedMilliseconds;
//             }
//     
//             var averageSeconds = totalMilliseconds / 1000f / Samples;
//             Debug.Log($"Mv took an average of {averageSeconds} seconds for {Samples} samples.");
//             
//         }
//         
//         [Test]
//         public void TimeMvToMirror()
//         {
//             long totalMilliseconds = 0;
//             var message = new TestMessage
//             {
//                 Int = 100,
//                 Long = long.MaxValue,
//                 Vec2 = new Vector2(2000, 3000)
//             };
//     
//             for (var i = 0; i < Samples; i++)
//             {
//                 var stopwatch = new Stopwatch();
//                 stopwatch.Start();
//                 for (var j = 0; j < ReadWrites; j++)
//                     _mvWriter.Write(message);
//     
//                 _mirrorWriter.WriteBytesAndSizeSegment(_mvWriter.GetData());
//                 stopwatch.Stop();
//                 _mvWriter.Reset();
//                 _mirrorWriter.Reset();
//                 totalMilliseconds += stopwatch.ElapsedMilliseconds;
//             }
//     
//             var averageSeconds = totalMilliseconds / 1000f / Samples;
//             Debug.Log($"Mv to Mirror took an average of {averageSeconds} seconds for {Samples} samples.");
//         }
//     
//         private void Time<TValue, TReader, TWriter>(
//             string name, Func<int, TValue> getValue,
//             Func<TWriter, TReader> getReader, TWriter writer,
//             Func<TReader, TValue> read, Action<TValue> write)
//         {
//             long totalMilliseconds = 0;
//     
//             for (var i = 0; i < Samples; i++)
//             {
//                 var stopwatch = new Stopwatch();
//                 stopwatch.Start();
//                 for (var j = 0; j < ReadWrites; j++)
//                     write(getValue(j));
//     
//                 var reader = getReader(writer);
//                 for (var j = 0; j < ReadWrites; j++)
//                     read(reader);
//     
//                 stopwatch.Stop();
//                 _mvWriter.Reset();
//                 _mirrorWriter.Reset();
//                 totalMilliseconds += stopwatch.ElapsedMilliseconds;
//             }
//     
//             var averageSeconds = totalMilliseconds / 1000f / Samples;
//             Debug.Log($"{name} took an average of {averageSeconds} seconds for {Samples} samples.");
//         }
//     
//         public class TestMessage : IMvMessage, NetworkMessage
//         {
//             public int Int;
//             public long Long;
//             public Vector2 Vec2;
//         }
//     }
// }