# Templ8or
Use template code to generate text data to speed up coding monotonous code
![Image of Application](https://i.ibb.co/F4FsJyF/image.png)
use "#declare VariableName:DataType" to create your initial variables
use ${[C# code goes here]}$  to execute c# code typically to say var="value" but any code works
use $var(this will be replaced in the text with your created variable value)


some example inputs and outputs
    Html Accordian panel (angular)
```csharp
#declare Id:string
#declare Title:string
${Id="Address-Panel"}$
${Title="Address"}$
<ngb-panel class="$Id" id="$Id">
      <ng-template ngbPanelTitle>
        <span for="$Id" class="$Id-header">$Title </span>
      </ng-template>
      <ng-template ngbPanelContent>
      	<span>content Goes here</span>
      </ng-template>
</ngb-panel>
```

---Result---
```csharp

<ngb-panel class="Address-Panel" id="Address-Panel">
      <ng-template ngbPanelTitle>
        <span for="Address-Panel" class="Address-Panel-header">Address </span>
      </ng-template>
      <ng-template ngbPanelContent>
      	<span>content Goes here</span>
      </ng-template>
</ngb-panel>
```
  html form group [angular]
  ```csharp
  #declare Label:string
#declare EntityName:string
#declare PropertyName:string
#declare AdditionalProperties:string
#declare InputType:String
${EntityName="Order"}$
${PropertyName="deliveryCharge"}$
${Label="Delivery Charge"}$
${InputType="text"}$
${AdditionalProperties = @"autocomplete=""off"" "}$
<div class="form-group">
          <label for="$EntityName-$PropertyName">$Label</label>
          <input name="$EntityName-$PropertyName" id="$EntityName-$PropertyName" type="$InputType" required class="form-control" [(ngModel)]="$EntityName.$PropertyName" />
</div>
```
---Output ----
```csharp
<div class="form-group">
          <label for="Order-deliveryCharge">Delivery Charge</label>
          <input name="Order-deliveryCharge" id="Order-deliveryCharge" type="text" required class="form-control" [(ngModel)]="Order.deliveryCharge" />
</div>

```
Project also has a rudimentary file system using the applications starting directory + "/Files" to store a folder file combo
```json
ex 
  Files
    Project A
      File A
      File B
     Project B
      File C
      File D
      ```
DO NOT name the files the same name (bug)

