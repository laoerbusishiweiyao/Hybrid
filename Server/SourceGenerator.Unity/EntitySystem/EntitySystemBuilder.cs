namespace SourceGenerator.Unity;

public static class EntitySystemBuilder
{
    private static readonly Dictionary<string, string> templates = new()
    {
        {
            "EntitySystem",
            """
            $attribute$
            public class $argsTypesUnderLine$_$methodName$System: $methodName$System<$argsTypes$>
            {   
                protected override $returnType$ $methodName$($argsTypesVars$)
                {
                    $return$$argsVars0$.$methodName$($argsVarsWithout0$);
                }
            }
            """
        },
        {
            "LockStepEntitySystem",
            """
            $attribute$
            public class $argsTypesUnderLine$_$methodName$System: $methodName$System<$argsTypes$>
            {   
                protected override void $methodName$($argsTypesVars$)
                {
                    $argsVars0$.$methodName$($argsVarsWithout0$);
                }
            }
            """
        },
        {
            "MessageHandler",
            """
                    $attribute$
            	    public class $className$_$methodName$_Handler: MessageHandler<$argsTypesWithout0$>
            	    {
            	    	protected override async MetaTask Run($argsTypesVars$)
            	    	{
                            await $className$.$methodName$($argsVars$);
                        }
                    }
            """
        },
        {
            "ActorMessageHandler",
            """
                    $attribute$
            	    public class $className$_$methodName$_Handler: ActorMessageHandler<$argsTypes$>
            	    {
            	    	protected override async MetaTask Run($argsTypesVars$)
            	    	{
                            await $className$.$methodName$($argsVars$);
                        }
                    }
            """
        },
        {
            "ActorMessageLocationHandler",
            """
                    $attribute$
            	    public class $className$_$methodName$_Handler: ActorMessageLocationHandler<$argsTypes$>
            	    {
            	    	protected override async MetaTask Run($argsTypesVars$)
            	    	{
                            await $className$.$methodName$($argsVars$);
                        }
                    }
            """
        },
        {
            "Event",
            """
            $attribute$
            public class $argsTypes2$_$methodName$: AEvent<$argsTypes$>
            {
                protected override async MetaTask Run($argsTypesVars$)
                {
                    await $className$.$methodName$($argsVars$);
                }
            }
            """
        },
    };

    public static string Find(string attributeType)
    {
        if (!templates.TryGetValue(attributeType, out var template) || template == null)
        {
            throw new Exception($"not config template: {attributeType}");
        }

        return template;
    }

    public static bool Contains(string attributeType)
    {
        return templates.ContainsKey(attributeType);
    }
}