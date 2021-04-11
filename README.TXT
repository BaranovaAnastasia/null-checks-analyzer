Null checks analyzer and code fix.

Finds checks for null in code and suggests removing them.

Can detect null checks like:

if (obj == null)
if (obj is null)
if (Object.ReferenceEquals(obj, null))
if (Object.ReferenceEquals(null, obj))
if (!(obj is object))
if (!(obj is Object))
obj = smth1 ?? smth2;
obj = smth1 == null ? smth2 : smth3;
obj = smth1 is null ? smth2 : smth3;
obj = Object.ReferenceEquals(smth1, null) ? smth2 : smth3;
obj = Object.ReferenceEquals(null, smth1) ? smth2 : smth3;
obj = !(smth1 is object) ? smth2 : smth3;
obj = !(smth1 is Object) ? smth2 : smth3;

And suggests to remove detected null checks as if the object being checked for null is never null.