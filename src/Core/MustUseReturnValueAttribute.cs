[AttributeUsage(
    AttributeTargets.Constructor
        | AttributeTargets.Method
        | AttributeTargets.Class
        | AttributeTargets.Struct
)]
class MustUseReturnValueAttribute : Attribute { }
