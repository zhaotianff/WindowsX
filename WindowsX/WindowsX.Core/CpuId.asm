option casemap:none

PUBLIC CpuIdQueryAsm
PUBLIC ReadXcr0Asm

.code

; Windows x64 ABI: ECX=leaf, EDX=subleaf, R8=register output.
; CPUID clobbers RBX, which must be preserved for the caller.
CpuIdQueryAsm PROC FRAME
	push rbx
	.pushreg rbx
	.endprolog
	mov eax, ecx
	mov ecx, edx
	cpuid
	mov DWORD PTR [r8], eax
	mov DWORD PTR [r8+4], ebx
	mov DWORD PTR [r8+8], ecx
	mov DWORD PTR [r8+12], edx
	pop rbx
	ret
CpuIdQueryAsm ENDP

; The caller must first check CPUID.1:ECX.XSAVE and OSXSAVE.
ReadXcr0Asm PROC
	xor ecx, ecx
	xgetbv
	shl rdx, 32
	or rax, rdx
	ret
ReadXcr0Asm ENDP

END
