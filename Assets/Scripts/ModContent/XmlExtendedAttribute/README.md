# XML Extended Attribute System

## Tổng quan

Hệ thống XML Extended Attribute cho phép xử lý các attribute tùy chỉnh trong XML files. Mỗi attribute sẽ được xử lý bởi một `XmlAttributeProcessor` tương ứng.

## Kiến trúc

```
XmlAttributeProcessorRegistry
    ├── ClassAttributeProcessor      (xử lý Class="TypeName")
    ├── GameObjectTargetAttributeProcessor (xử lý GameObjectTarget="Type:Id")
    ├── ConditionalAttributeProcessor    (xử lý Condition="Expression")
    └── Custom processors...         (thêm mới theo nhu cầu)
```

## Cách tạo Processor mới

### Bước 1: Tạo class kế thừa XmlAttributeProcessor

```csharp
using System.Xml;
using ModContent.XmlExtendedAttribute;

public class MyCustomProcessor : XmlAttributeProcessor
{
    // Tên attribute trong XML
    public override string AttributeName => "MyAttribute";
    
    // Thứ tự xử lý (thấp hơn = chạy trước)
    public override int Priority => 0;

    // Kiểm tra có thể xử lý không (optional)
    public override bool CanProcess(XmlNode node, string value)
    {
        // Validation trước khi process
        return !string.IsNullOrEmpty(value);
    }

    // Xử lý attribute
    public override void Process(XmlAttributeProcessContext context)
    {
        // Đọc giá trị
        string value = context.Value;
        
        // Ghi log
        context.Log($"Processing: {value}");
        context.LogWarning($"Warning: {value}");
        context.LogError($"Error: {value}");
        
        // Chia sẻ state với processor khác
        context.SetState("MyKey", parsedValue);
        var otherValue = context.GetState<string>("OtherKey");
        
        // Truy cập node gốc
        XmlNode node = context.Node;
        
        // Truy cập settings
        XmlConverterSettings settings = context.Settings;
    }
}
```

### Bước 2: Đăng ký Processor

```csharp
// Cách 1: Đăng ký vào XmlConverterSettings.Default
XmlConverterSettings.Default.RegisterAttributeProcessor(new MyCustomProcessor());

// Cách 2: Đăng ký vào registry riêng
var registry = new XmlAttributeProcessorRegistry();
registry.Register(new MyCustomProcessor());

// Cách 3: Đăng ký nhiều processor cùng lúc
settings.RegisterAttributeProcessors(new List<XmlAttributeProcessor>
{
    new MyCustomProcessor(),
    new AnotherProcessor()
});
```

### Bước 3: Sử dụng trong XML

```xml
<Data>
    <Define MyAttribute="myValue">
        <Id>test</Id>
    </Define>
</Data>
```

## Built-in Processors

### 1. ClassAttributeProcessor
- **Attribute**: `Class`
- **Priority**: 0 (chạy đầu tiên)
- **Mục đích**: Resolve type từ tên class
- **Ví dụ**: `Class="ProvideQualities_CompProperties"`

### 2. GameObjectTargetAttributeProcessor
- **Attribute**: `GameObjectTarget`
- **Priority**: 10
- **Mục đích**: Đăng ký mapping Id → Type
- **Ví dụ**: `GameObjectTarget="Creature:HumanDef"`

### 3. ConditionalAttributeProcessor
- **Attribute**: `Condition`
- **Priority**: -10 (chạy rất sớm)
- **Mục đích**: Conditional loading
- **Ví dụ**: `Condition="IsDevMode==true"`

## State Sharing

Các processor có thể chia sẻ dữ liệu qua `context.State`:

```csharp
// Processor A
context.SetState("ParsedType", typeof(Creature));

// Processor B
Type parsedType = context.GetState<Type>("ParsedType");
```

## Priority System

- **< 0**: Chạy rất sớm (conditional checks, validation)
- **0**: Mặc định
- **> 0**: Chạy sau (dependent processors)
- **1000**: Legacy adapters

## Migration từ旧系统

旧 interface `IXmlExtendedAttribute` vẫn được hỗ trợ qua `LegacyAttributeAdapter`:

```csharp
//旧 code
public class MyOldAttribute : IXmlExtendedAttribute { ... }

//新 code - wrap旧attribute
var adapter = new LegacyAttributeAdapter(new MyOldAttribute());
settings.RegisterAttributeProcessor(adapter);
```

## Example: Tạo processor cho RequireLevel

```csharp
public class RequireLevelProcessor : XmlAttributeProcessor
{
    public override string AttributeName => "RequireLevel";
    public override int Priority => 5;

    public override void Process(XmlAttributeProcessContext context)
    {
        if (int.TryParse(context.Value, out int level))
        {
            context.SetState("RequiredLevel", level);
            context.Log($"Require level: {level}");
        }
        else
        {
            context.LogError($"Invalid level: {context.Value}");
        }
    }
}
```
