## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaAA()
       movzx     eax,word ptr [rcx+8]
       lea       edx,[rax+0FF9F]
       cmp       rdx,19
       jle       short M00_L00
       add       eax,0FFFFFFBF
       cmp       rax,19
       setle     al
       movzx     eax,al
       jmp       short M00_L01
M00_L00:
       mov       eax,1
M00_L01:
       ret
; Total bytes of code 34
```

## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaAB()
       movzx     eax,word ptr [rcx+8]
       lea       edx,[rax+0FF9F]
       cmp       rdx,19
       setle     dl
       movzx     edx,dl
       add       eax,0FFFFFFBF
       cmp       rax,19
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 36
```

## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaB()
       movzx     eax,word ptr [rcx+8]
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       ret
; Total bytes of code 24
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaAA()
       movzx     eax,word ptr [rcx+8]
       lea       edx,[rax+0FF9F]
       cmp       rdx,19
       jle       short M00_L00
       add       eax,0FFFFFFBF
       cmp       rax,19
       setle     al
       movzx     eax,al
       jmp       short M00_L01
M00_L00:
       mov       eax,1
M00_L01:
       ret
; Total bytes of code 34
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaAB()
       movzx     eax,word ptr [rcx+8]
       lea       edx,[rax+0FF9F]
       cmp       rdx,19
       setle     dl
       movzx     edx,dl
       add       eax,0FFFFFFBF
       cmp       rax,19
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 36
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.AlphaCheckMicrobenchmark.IsAlphaB()
       movzx     eax,word ptr [rcx+8]
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       ret
; Total bytes of code 24
```

