import { Component, ElementRef, forwardRef, Input, Self} from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, NG_VALUE_ACCESSOR ,ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css'],

})

export class TextInputComponent implements ControlValueAccessor {

 @Input()  label!: string;
 @Input() type:string ='text';

  value: any;
  class:any
  onChange!: (arg0: any) => void;
  onTouched!: () => void;

  constructor(private elementRef:ElementRef,public control:NgControl){
    if (this.control != null) {
      this.control.valueAccessor = this;
    }
  }
  change(value: any) {
    this.value = value;
    this.onChange(value);
  }
  blur()
  {
    this.onTouched();
  }

  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn:any): void {
   this.onTouched = fn;
  }


}