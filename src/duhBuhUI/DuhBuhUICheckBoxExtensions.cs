using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Checkbox registration surface for the reusable duhBuhUI library.
// The registration is kept isolated from the proven toggle implementation so the
// checkbox can be validated without disturbing existing controls.
public static class DuhBuhUICheckBoxExtensions
{
    public static void AddCheckBox(this DuhBuhUI ui, string title, string description, string category, string variableName, bool defaultValue)
    {
        if (ui == null) throw new ArgumentNullException("ui");

        Type uiType = typeof(DuhBuhUI);
        FieldInfo defaultsField = uiType.GetField("_defaults", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo controlsField = uiType.GetField("_controls", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo categoriesField = uiType.GetField("_categories", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo orderField = uiType.GetField("_definitionOrder", BindingFlags.Instance | BindingFlags.NonPublic);
        if (defaultsField == null || controlsField == null || categoriesField == null || orderField == null)
            throw new InvalidOperationException("duhBuhUI internals required for checkbox registration were not found.");

        IDictionary defaults = (IDictionary)defaultsField.GetValue(ui);
        IList controls = (IList)controlsField.GetValue(ui);
        IList categories = (IList)categoriesField.GetValue(ui);

        string resolvedCategory = string.IsNullOrEmpty(category) ? "General" : category;
        if (!categories.Contains(resolvedCategory)) categories.Add(resolvedCategory);
        defaults[variableName] = defaultValue;

        Type controlDefinitionType = controls.GetType().GetGenericArguments()[0];
        object definition = Activator.CreateInstance(controlDefinitionType);
        controlDefinitionType.GetField("Type").SetValue(definition, "checkbox");
        controlDefinitionType.GetField("Title").SetValue(definition, title);
        controlDefinitionType.GetField("Description").SetValue(definition, description);
        controlDefinitionType.GetField("Category").SetValue(definition, resolvedCategory);
        controlDefinitionType.GetField("Key").SetValue(definition, variableName);
        controlDefinitionType.GetField("DefaultValue").SetValue(definition, defaultValue);
        controlDefinitionType.GetField("Order").SetValue(definition, (int)orderField.GetValue(ui));
        orderField.SetValue(ui, (int)orderField.GetValue(ui) + 1);
        controls.Add(definition);
    }
}
