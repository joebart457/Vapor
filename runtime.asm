format PE console
entry _start

include 'C:\Users\Jimmy Barnes\Documents\UnnamedLanguage\Unnamed.Cmd\bin\Debug\net6.0\fasm\INCLUDE\macro\proc64.inc'
include 'C:\Users\Jimmy Barnes\Documents\UnnamedLanguage\Unnamed.Cmd\bin\Debug\net6.0\fasm\INCLUDE\win64ax.inc'

MAGIC_NUMBER equ 0x0000039A

section '.data' data readable writeable
	_hHeap     dq 0
	_insPtr    dq _code
	_vStack    db STACK_SIZE
	_vStackPtr dq 0
	_fnManifest dq 0
	dq print
	
	macro next_instruction
	{
		mov rsi, _code
		add rsi, qword [_insPtr]
		mov bl, byte [rsi]
		add qword [_insPtr], 1
	}
	
	
section '.text' code readable executable

	_start:
        invoke GetProcessHeap 
        test eax, eax
        jz FAIL_ALLOC
        mov [_hHeap], eax
        mov eax, dword 0
        stdcall vm_run
        push rax
        call [ExitProcess]
        FAIL_ALLOC:
        push 1001
        call [ExitProcess]
        FAIL_HEAP_FREE:
        push 1002
        call [ExitProcess]
        FAIL_NULL_PTR:
        push 1003
        call [ExitProcess]
        FAIL_DIVISION_BY_ZERO:
        push 1004
        call [ExitProcess]
        FAIL_INDEX_OUT_OF_RANGE:
        push 1005
        call [ExitProcess]
        NO_ROOM:
        push 1006
        call [ExitProcess]
        NO_ROOM_REF:
        push 1007
        call [ExitProcess]
        NOT_FOUND:
        push 1008
        call [ExitProcess]
		FAIL_DARK_MAGIC:
		push 1009
		call [ExitProcess]
		FAIL_INIT:
		push 1010
		call [ExitProcess]
		VSTACK_UNDERFLOW:
		push 1011
		call [ExitProcess]
		ISTACK_OVERFLOW:
		push 1012
		call [ExitProcess]
		ISTACK_UNDERFLOW:
		push 1013
		call [ExitProcess]


proc vm_run stdcall
	stdcall validate_init
	.next:
		stdcall next_instruction
		cmp al, 1
		jl .process_push
		je .process_pushs
		cmp al, 2
		je .process_call
		; return invalid instruction
		mov rax, -1
		ret
	
	.process_push:
		call istack_pop
		call vstack_push
		jmp .next
	
	.process_pushs:
		call istack_pop
		add rax, _code ; add offset to address of _code
		call vstack_push
		jmp .next
	
	.process_call:
		call istack_pop
		call perform_call
		jmp .next	
	VM_FINISH:
	xor rax, rax
	ret
endp

; returns next instruction on al
proc next_instruction stdcall
	mov rsi, [_insPtr]
	cmp rsi, _endcode
	jge VM_FINISH
	
	mov al, byte [rsi]
	inc rsi
	mov [_insPtr], rsi
	ret
endp

proc validate_init stdcall
	
	mov rax, _insPtr
	sub rax, _code+8
	jge FAIL_INIT
	
	
	mov rsi, _code
	mov rax, qword [_code]
	cmp rax, MAGIC_NUMBER
	jne FAIL_DARK_MAGIC
	add rsi, 8
	mov _insPtr, [rsi]
	ret
endp

vstack_pop:
	mov rdi, [_vStackPtr]
	sub rdi, 8
	cmp rdi, _vStack
	jle VSTACK_UNDERFLOW
	mov rax, qword [rdi]
	ret

vstack_push:
	mov rdi, [_vStackPtr]
	cmp rdi, _vStack+STACK_SIZE-8
	jg VSTACK_OVERFLOW
	mov [rdi], rax
	add rdi,8 
	mov [_vStackPtr], rdi
	ret
	
istack_pop:
	mov rsi, _insPtr
	cmp rsi, _code
	jle ISTACK_UNDERFLOW
	mov rax, qword [rsi]
	add rsi, 8
	cmp rsi, _endcode
	jge ISTACK_OVERFLOW
	mov [_insPtr], rsi
	ret

perform_call:
	mov rcx, _fnManifest
	add rcx, rax
	
proc print stdcall 
	vstack_pop
	cinvoke printf, rax
	ret
endp


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

