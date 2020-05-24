
PUBLIC ThisPtrCall
PUBLIC SnatcherModuleHandle

.data

	SnatcherModuleHandle dq 0

.code

	; __int64 __stdcall ThisPtrCall(void *functionPtr, void *thisPtr, void *arg1, void *arg2, void *arg3, void *arg4)
	ThisPtrCall PROC

		; Setup the stack.
		push	r12
		push	r14

		; Calculate the correct function address.
		mov		r12, rcx
		mov		r14, 140000000h
		sub		r12, r14
		mov		r14, SnatcherModuleHandle
		add		r12, r14

		; Shift the arguments.
		mov		rcx, rdx
		mov		rdx, r8
		mov		r8, r9
		mov		r9, [rsp+38h]

		; Call the function.
		call	r12

		; Restore the stack.
		pop		r14
		pop		r12
		ret

	ThisPtrCall ENDP

END