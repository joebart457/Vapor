
include 'C:\Users\Jimmy Barnes\Documents\UnnamedLanguage\Unnamed.Cmd\bin\Debug\net6.0\fasm\INCLUDE\macro\proc64.inc'
include 'C:\Users\Jimmy Barnes\Documents\UnnamedLanguage\Unnamed.Cmd\bin\Debug\net6.0\fasm\INCLUDE\win64ax.inc'

MAGIC_NUMBER equ 0x0000039A

section '.data' data readable writeable
	_hHeap     dq 0
	_insPtr    dq 0
	_fnManifest dq 0
	dq print
	dq navigate_code

macro istack_pop {
	mov rsi, [_insPtr]
	cmp rsi, _code
	jle ISTACK_UNDERFLOW
	mov rax, qword [rsi]
	add rsi, 8
	cmp rsi, _endcode
	jg ISTACK_OVERFLOW
	mov [_insPtr], rsi

}

; returns next instruction on al
macro next_instruction {
	mov rsi, [_insPtr]
	cmp rsi, _endcode
	je VM_FINISH
	jg ISTACK_OVERFLOW
	cmp rsi, _code
	jl ISTACK_UNDERFLOW

	mov al, byte [rsi]
	inc rsi
	mov [_insPtr], rsi
}

macro perform_call {

	mov rcx, _fnManifest
	add rcx, rax
	call qword [rcx]
}	


section '.text' code readable executable

	_start:
        xor rax, rax
        call vm_run
        invoke ExitProcess, rax
		FAIL_DARK_MAGIC:
		invoke ExitProcess, 1
		FAIL_INIT:
		invoke ExitProcess, 2
		ISTACK_OVERFLOW:
		invoke ExitProcess, 8
		ISTACK_UNDERFLOW:
		invoke ExitProcess, 7

vm_run:
	push rbp
	mov rbp, rsp
	call validate_init
	.next:
		next_instruction
		cmp al, 1
		jl .process_push
		je .process_pushs
		cmp al, 2
		je .process_call
		; return invalid instruction
		
		mov rax, -1
		ret
	
	.process_push:
		istack_pop
		push rax
		jmp .next
	
	.process_pushs:
		istack_pop
		add rax, _code ; add offset to address of _code
		push rax
		jmp .next
	
	.process_call:
		istack_pop
		perform_call
		jmp .next	
		
	VM_FINISH:
	mov rsp, rbp
	pop rbp
	xor rax, rax
	ret


validate_init:
	
	mov rax, _endcode
	cmp rax, _code+8
	jle FAIL_INIT
	
	mov rsi, _code
	mov rax, qword [_code]
	cmp rax, MAGIC_NUMBER
	jne FAIL_DARK_MAGIC
	add rsi, 8
	mov rbx, qword [rsi]
	add rbx, _code ; since 2nd qword in byte code is offset of instructions from begining of bytecode
	mov [_insPtr], rbx
	ret

print:
	push rbp            ; Store the current stack frame
    mov  rbp, rsp       ; Preserve RSP into RBP for argument references
    mov  rax, [rbp+16]   ; Move the contents of EBP+8 into EAX
                        ; [RBP] should be the saved 32 bit EBP.
                        ; [RBP+8] should be the 64 bit RIP (return address).
                        ; [RBP+16] should be the pushed parameter.
    
	cinvoke printf, rax
	
    mov  rsp, rbp       ; Restore the stack and rbp
    pop  rbp
    ret 8				; cleanup parameter

navigate_code:
	push rbp            ; Store the current stack frame
    mov  rbp, rsp       ; Preserve RSP into RBP for argument references
    mov  rbx, [rbp+16]   ; Move the contents of EBP+8 into EAX
                        ; [RBP] should be the saved 32 bit EBP.
                        ; [RBP+8] should be the 64 bit RIP (return address).
                        ; [RBP+16] should be the pushed parameter.
    
	mov rsi, qword [_code + 8]
	add rsi, _code
	mov rax, 9
	imul rbx 		; since instructions are saving in chunks of 9 bytes
	add rsi, rax
	mov [_insPtr], rsi
    mov  rsp, rbp       ; Restore the stack and rbp
    pop  rbp
    ret 8				; cleanup parameter


section '.idata' import data readable
    library \
        kernel32, 'kernel32.dll', \
        msvcrt, 'msvcrt.dll'
    import kernel32, \
        GetProcessHeap, 'GetProcessHeap', \
        ExitProcess, 'ExitProcess', \
        HeapAlloc, 'HeapAlloc', \
        HeapFree, 'HeapFree'
    import msvcrt, \
        printf, 'printf', \
        _getch!ty!int32!, '_getch'

