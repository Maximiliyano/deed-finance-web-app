import { Component, DestroyRef, OnInit, forwardRef, inject, Input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Tag } from '../../../../modules/expense/models/tag';

@Component({
  selector: 'app-selective-input',
  templateUrl: './selective-input.component.html',
  styleUrl: './selective-input.component.scss',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectiveInputComponent),
      multi: true
    }
  ],
  imports: [CommonModule, ReactiveFormsModule],
})
export class SelectiveInputComponent implements ControlValueAccessor, OnInit {
  private readonly destroyRef = inject(DestroyRef);

  @Input() suggestions: Tag[] = [];
  @Input() placeholder = 'Add tag...';
  
  filteredSuggestions: Tag[] = [];
  inputCtrl = new FormControl('');
  selectedTags: string[] = [];
  showDropdown = false;
  inputValue = '';
  
  private onChange: (value: string[]) => void = () => {};
  private onTouched: () => void = () => {};
  
  ngOnInit(): void {
    this.inputCtrl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(value => {
        this.inputValue = value ?? '';
        this.filterSuggestions();
      });
  }

  private filterSuggestions(): void {
    const term = this.inputValue.toLowerCase();

    this.filteredSuggestions = this.suggestions.filter(tag =>
      tag.name.toLowerCase().includes(term) &&
      !this.selectedTags.some(name => name === tag.name)
    );

    this.showDropdown = this.filteredSuggestions.length > 0;
  }

  writeValue(value: string[]): void {
    this.selectedTags = value ?? [];
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  onFocus() {
    this.filterSuggestions();
  }

  onBlur() {
    this.onTouched();
    setTimeout(() => {
      this.showDropdown = false;
    });
  }

  selectTag(tagName: string) {
    if (this.selectedTags.includes(tagName)) return;

    this.selectedTags = [...this.selectedTags, tagName];
    
    this.inputCtrl.reset();
    this.onChange(this.selectedTags);
    this.filterSuggestions();
  }

  addFromInput(event: KeyboardEvent) {
    if (event.key !== 'Enter') return;
    event.preventDefault();

    const value = this.inputCtrl.value?.trim();
    if (!value) return;

    this.selectTag(value);
    this.inputCtrl.reset();
  }

  removeTag(tagName: string) {
    this.selectedTags = this.selectedTags.filter(name => name !== tagName);
    this.onChange(this.selectedTags);
  }
}
