## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericA()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       add       edx,0FFFFFFD0
       cmp       rdx,9
       jle       short M00_L00
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       ret
M00_L00:
       mov       eax,1
       ret
; Total bytes of code 41
```

## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericB()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       add       edx,0FFFFFFD0
       cmp       rdx,9
       setle     dl
       movzx     edx,dl
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 44
```

## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericC()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       or        edx,20
       movzx     edx,dx
       add       edx,0FFFFFF9F
       cmp       rdx,19
       jle       short M00_L00
       add       eax,0FFFFFFD0
       cmp       rax,9
       setle     al
       movzx     eax,al
       ret
M00_L00:
       mov       eax,1
       ret
; Total bytes of code 41
```

## .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericD()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       or        edx,20
       movzx     edx,dx
       add       edx,0FFFFFF9F
       cmp       rdx,19
       setle     dl
       movzx     edx,dl
       add       eax,0FFFFFFD0
       cmp       rax,9
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 44
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericA()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       add       edx,0FFFFFFD0
       cmp       rdx,9
       jle       short M00_L00
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       ret
M00_L00:
       mov       eax,1
       ret
; Total bytes of code 41
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericB()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       add       edx,0FFFFFFD0
       cmp       rdx,9
       setle     dl
       movzx     edx,dl
       or        eax,20
       movzx     eax,ax
       add       eax,0FFFFFF9F
       cmp       rax,19
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 44
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericC()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       or        edx,20
       movzx     edx,dx
       add       edx,0FFFFFF9F
       cmp       rdx,19
       jle       short M00_L00
       add       eax,0FFFFFFD0
       cmp       rax,9
       setle     al
       movzx     eax,al
       ret
M00_L00:
       mov       eax,1
       ret
; Total bytes of code 41
```

## .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
```assembly
; Loretta.InternalBenchmarks.IsAlphaNumericMicrobenchmark.IsAlphaNumericD()
       movzx     eax,word ptr [rcx+8]
       mov       edx,eax
       or        edx,20
       movzx     edx,dx
       add       edx,0FFFFFF9F
       cmp       rdx,19
       setle     dl
       movzx     edx,dl
       add       eax,0FFFFFFD0
       cmp       rax,9
       setle     al
       movzx     eax,al
       or        eax,edx
       movzx     eax,al
       ret
; Total bytes of code 44
```