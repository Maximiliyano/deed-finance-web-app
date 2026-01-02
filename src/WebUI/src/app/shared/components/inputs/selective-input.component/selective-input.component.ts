import { Component, EventEmitter, forwardRef, Input, Output } from '@angular/core';
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
export class SelectiveInputComponent implements ControlValueAccessor {
  @Input() suggestions: Tag[] = [];
  @Input() placeholder = 'Add tag...';
  
  @Output() searchChange = new EventEmitter<string>();
  @Output() createChange = new EventEmitter<Tag[]>();
  
  filteredSuggestions: Tag[] = [];
  inputCtrl = new FormControl('');
  selectedTags: Tag[] = [];
  showDropdown = false;
  inputValue = '';
  
  private onChange: (value: Tag[]) => void = () => {};
  private onTouched: () => void = () => {};
  
  constructor() {
    this.inputCtrl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(value => {
        this.inputValue = value ?? '';
        this.filterSuggestions();
        this.searchChange.emit(this.inputValue);
      });
  }

  private filterSuggestions(): void {
    const term = this.inputValue.toLowerCase();

    this.filteredSuggestions = this.suggestions.filter(tag =>
      tag.name.toLowerCase().includes(term) &&
      !this.selectedTags.some(selected => selected.tagId === tag.tagId)
    );

    this.showDropdown = this.filteredSuggestions.length > 0;
  }

  writeValue(value: Tag[]): void {
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
    setTimeout(() => {
      this.showDropdown = false;
    });
  }

  onSelectSuggestion(tag: Tag) {
    // select already added
    if (this.selectedTags.some(t => t.tagId === tag.tagId)) return;

    this.selectedTags = [...this.selectedTags, tag];

    this.onChange(this.selectedTags);
    this.filterSuggestions();
  }

  addFromInput(event: KeyboardEvent) {
    if (event.key !== 'Enter') return;
    event.preventDefault();

    const value = this.inputCtrl.value?.trim();
    if (!value) return;

    this.onSelectSuggestion({ tagId: 0, expenseId: 0, name: value });
    this.inputCtrl.reset();

    this.createChange.emit(this.selectedTags);
  }

  removeTag(tag: Tag) {
    this.selectedTags = this.selectedTags.filter(t => t.tagId !== tag.tagId);
    this.onChange(this.selectedTags);
  }

  private resetInput() {
    this.inputCtrl.reset();
    this.showDropdown = false;
    this.filteredSuggestions = [...this.suggestions]
  }
}
