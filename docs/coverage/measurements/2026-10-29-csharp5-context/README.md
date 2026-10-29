# Production CSharp context measurements

Release, SDK10.0.100, .NET10.0.0. Baseline CSNitrogen source:12bab5a78793ffb179b0454f22692df475fb5cfe; candidate:95b9c5bd4b310b7d2ed0095dd46119d96112e77b. Both builds use Nitrogen source b3494870dfb5f187bae9065227d4ef7f742aa048. Baseline deliberately overrides its original dependency to hold the runtime source constant; it is a measurement reference, not a new admission claim for that overridden build. Candidate uses the default141-source pinned snapshot.

Harness: eng/csharp5-context-benchmark. Each case warms1000 operations; incremental cases warm1000 each round. Seven rounds retain all elapsed times and thread allocations. Full cases use500 iterations and incremental2000. The sole incremental change replaces the first class-name character; inputs must pass every syntax/policy check. Baseline then candidate run sequentially with this task's builds/tests idle. Host editor/other workloads remain uncontrolled and tiered JIT remains enabled.

| Case | Baseline median ms/op | Candidate median ms/op | Baseline bytes/op | Candidate bytes/op |
|---|---:|---:|---:|---:|
| CSharp4/raw | 0.277920 | 0.270314 | 640 | 640 |
| CSharp4/policy | 0.482577 | 0.513105 | 602252 | 640767 |
| CSharp4/incremental-policy | 0.158320 | 0.161366 | 228119 | 229839 |

Paired cases retain1322 tree nodes; incremental reparsing retains1313 reused nodes. Raw parsing allocation stays640 bytes/op. Full source-checking allocation increases by38515 bytes/op (about6.4%); incremental source checking increases1720 bytes/op. Raw memo hits stay525, while misses increase1718->2348 with the added shared entry probes. These are observed costs; no stable throughput parity, retained-memory bound or editor latency claim follows from short rounds with promotion/host noise.

Candidate-only mixed CSharp5 input is1790 UTF16 characters,10 generic classes with async methods/lambdas and ordinary await invocations. Full parsing produces1642 nodes; the class-header reparse reuses1633. Its full median is0.707595 ms/op and incremental median0.208214 ms/op. There is no old CSharp5 feature implementation to compare. These are syntax/source-policy executions, not historical source admission or semantic compilation.

## Artifact identity

- baseline ContextBenchmark.dll: SHA256 `f96daf110fea67b5b1dea0a37fdf6af9c316d2ca1d57e98b7c9e5dfedfd1383a`
- baseline CSNitrogen.dll: SHA256 `f2ec5bfe93bc44af8d1d22d8e54c54a81430874f0e0946da81a7ca48482c83fd`
- baseline CSNitrogen.Syntax.dll: SHA256 `301faacc87564f2f8fbd2e09fa7543d4941d294e80c97e008b8bc8d19ae8ccc2`
- baseline Nitrogen.Runtime.dll: SHA256 `65873c09d8fd39f17707ded3929ab0e1c57f62acab1301aca472c2e5475269cd`
- candidate ContextBenchmark.dll: SHA256 `c6286dda6dd7f559e51a914f832afb550ec3851550b29c5d9f2f5bf5e2dfdb2a`
- candidate CSNitrogen.dll: SHA256 `1666f37e3a36a4d342817e4469539d568b2ea8dcc120bf2cffa0b6a5d74913d8`
- candidate CSNitrogen.Syntax.dll: SHA256 `9702a0408756165b56e9239d52374abdc838f32eedb1aee2a375a2585bacdc96`
- candidate Nitrogen.Runtime.dll: SHA256 `afa623a19ed7264c52f31f480b65ec7ed522c22159f87cab9a32e1f8fc19f629`
- Harness Program.cs: SHA256 `1bf69b90dd64c67e0ff1bcb2d0db5d287eb3e15dd496666bc09a918786355e26`
