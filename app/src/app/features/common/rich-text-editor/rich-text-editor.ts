import {
  AfterViewChecked,
  Component,
  forwardRef,
  input,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { QuillEditorComponent, QuillModule } from 'ngx-quill';
import { Subject, take } from 'rxjs';

@Component({
  selector: 'app-rich-text-editor',
  imports: [QuillModule],
  templateUrl: './rich-text-editor.html',
  styleUrl: './rich-text-editor.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: forwardRef(() => RichTextEditor),
    },
  ],
})
export class RichTextEditor implements AfterViewChecked, ControlValueAccessor {
  private _editor = viewChild.required(QuillEditorComponent);

  private readonly _writeValueChanges = new Subject<string>();

  private readonly _defaultConfig: Record<string, any> = {
    clipboard: {
      matchVisual: false,
    },
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ list: 'ordered' }, { list: 'bullet' }],
      ['link'],
      ['clean'],
    ],
  };

  public config = input<Record<string, any>>(this._defaultConfig);
  public textAreaHeight = input<string>('258px');
  public readOnly = input<boolean>(false);
  public placeHolderText = input<string>('');

  public readonly formats: string[] = [
    'background',
    'bold',
    'color',
    'font',
    'code',
    'italic',
    'link',
    'size',
    'strike',
    'script',
    'underline',
    'blockquote',
    'header',
    'indent',
    'list',
    'align',
    'direction',
    'formula',
    // 'image'
    // 'video'
  ];

  private _isDisabled = false;
  private _editorInitialValue: string | undefined;
  private _rawInitialValue: string | undefined;
  private _hasInitEditor = false;

  constructor() {
    const firstChange$ = this._writeValueChanges
      .pipe(take(1), takeUntilDestroyed())
      .subscribe((rawInitValue) => (this._rawInitialValue = rawInitValue));
  }

  ngAfterViewChecked(): void {
    this._editorInitialValue = this._editor().content;
    this._hasInitEditor = true;
  }

  writeValue(value: any): void {
    this._editor().writeValue(value);
    this._writeValueChanges.next(value);
  }

  registerOnChange(changeFn: any): void {
    this._editor().registerOnChange((newValue: string) => {
      if (!this._hasInitEditor) {
        return;
      }

      if (newValue === this._editorInitialValue) {
        changeFn(this._rawInitialValue);
      } else {
        changeFn(newValue);
      }
    });
  }

  registerOnTouched(touchedFn: any): void {
    this._editor().registerOnTouched(touchedFn);
  }

  setDisabledState?(isDisabled: boolean): void {
    this._editor().setDisabledState(this._isDisabled);
  }
}
