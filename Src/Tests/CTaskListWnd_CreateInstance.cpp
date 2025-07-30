// Hidden C++ exception states: #wind=1
__int64 __fastcall CTaskListWnd_CreateInstance(struct IObjectFactory *a1, IUnknown **ppunk)
{
  unsigned int v4; // ebx
  CTaskListWnd *v5; // rax
  CTaskListWnd *v6; // rdi
  IUnknown *v7; // rax
  IUnknown *v8; // rdi

  *ppunk = 0;
  v4 = -2147024882;
  v5 = (CTaskListWnd *)operator new(0x2B8u);
  v6 = v5;
  if ( v5 )
  {
    memset_0(v5, 0, 0x2B8u);
    v7 = (IUnknown *)CTaskListWnd::CTaskListWnd(v6, a1);
    v8 = v7;
    if ( v7 )
    {
      IUnknown_Set(ppunk, v7 + 5);
      CTaskUnknown::Release((CTaskUnknown *)&v8[2]);
      return 0;
    }
  }
  return v4;
}
