import { Component, Input, OnInit } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.css']
})
export class DateInputComponent implements ControlValueAccessor {
  @Input()  label!: string;
  @Input()  maxDate!: Date;
  bsConfig!: Partial<BsDatepickerConfig>;

  value: any;
  class:any
  onChange!: (arg0: any) => void;
  onTouched!: () => void;
  
  constructor(public control:NgControl) { 
    if (this.control != null) {
      this.control.valueAccessor = this;
      this.bsConfig={
        containerClass:'theme-red',
        dateInputFormat:'DD MMMM YYYY'
      }
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
