import { Dialog } from '@angular/cdk/dialog';
import {
  Component,
  computed,
  effect,
  inject,
  linkedSignal,
  Signal,
  signal,
  untracked,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SelectButton } from 'primeng/selectbutton';
import { firstValueFrom } from 'rxjs';
import { SpecSheetPhaseHelpers } from '../../../../core/helpers/spec-sheet-phase-helpers';
import { Dataset } from '../../../../core/models/analytics/dataset';
import { DatasetColumn } from '../../../../core/models/analytics/dataset-column';
import { DatasetColumnType } from '../../../../core/models/analytics/dataset-column-type.enum';
import { PUSpecSheetSourceConfiguration } from '../../../../core/models/analytics/pu-spec-sheet-source-configuration';
import { TableCellType } from '../../../../core/models/domain/template/table-cell-type';
import { CellLookupService } from '../../../../core/services/cell-lookup.service';
import { DatasetCommunicationService } from '../../../../core/services/dataset-communication.service';
import { PhasesService } from '../../../../core/services/phases.service';
import { TypesService } from '../../../../core/services/types.service';
import { ConfirmationDialog } from '../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { Destroyable } from '../../../common/destructable/destructable';
import { ElementLabel } from '../../../common/element-label/element-label';
import { PUSpecSheetTableCellBase } from '../../../sheet/models/cells/pu-spec-sheet-table-cell-base';
import { ActiveSheetService } from '../../../sheet/services/active-sheet.service';
import { SelectionService } from '../../../sheet/services/selection.service';
import { ISidePanelLayer } from '../../../side-panel/models/side-panel-layer-data.interface';
import { PUCardAnalyticsService } from '../../services/pu-card-analytics.service';
import { AnalyticsDataPointConfigurationService } from './analytics-data-point-configuration.service';
import { DataPointControl } from './components/data-point-control/data-point-control';

@Component({
  selector: 'app-analytics-data-point-configuration',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ElementLabel,
    DataPointControl,
    SelectButton,
  ],
  templateUrl: './analytics-data-point-configuration.html',
  styleUrl: './analytics-data-point-configuration.scss',
  providers: [AnalyticsDataPointConfigurationService],
})
export class AnalyticsDataPointConfiguration
  extends Destroyable
  implements ISidePanelLayer<any>
{
  private static _sourceChangeConfirmationOpen = false;
  private static _lastPromptedSourceConfigurationChangeKey: string | null =
    null;

  public readonly data!: any;
  public readonly title = signal<string>('Manage Analytics Data Points');
  public readonly selectButtonOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false },
  ];
  public readonly specSheetTypeSelected = computed(() => {
    const dataset = this._analyticsService.currentDataset();
    const specSheetTypes = this._typesService.specSheetTypes();
    const openSheet = this._activeSheetService.specSheet();
    if (
      !dataset ||
      !specSheetTypes ||
      specSheetTypes.length === 0 ||
      !openSheet
    ) {
      return undefined;
    }

    const typeId =
      openSheet.specSheetType().id ?? dataset.sourceConfiguration.typeId;
    return specSheetTypes.find((p) => p.id === typeId);
  });
  public readonly specSheetPhaseSelected = computed(() => {
    const dataset = this._analyticsService.currentDataset();
    const specSheetPhases = this._phasesService.specSheetPhases();
    const openSheet = this._activeSheetService.specSheet();
    if (
      !dataset ||
      !specSheetPhases ||
      specSheetPhases.length === 0 ||
      !openSheet
    ) {
      return undefined;
    }

    const phaseId =
      openSheet.specSheetPhase().id ?? dataset.sourceConfiguration.phaseId;
    return SpecSheetPhaseHelpers.getPhaseByIdFromArray(
      specSheetPhases,
      phaseId,
    );
  });
  public readonly useLatestVersion = linkedSignal(() => {
    const dataset = this._analyticsService.currentDataset();
    return dataset?.sourceConfiguration.useLatestVersion ?? false;
  });

  private readonly _analyticsService = inject(PUCardAnalyticsService);
  private readonly _datasetCommunicationService = inject(
    DatasetCommunicationService,
  );
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _typesService = inject(TypesService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _selectionService = inject(SelectionService);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _dialog = inject(Dialog);

  constructor() {
    super();

    effect(() => {
      const dataset = this._analyticsService.currentDataset();
      const ready = this._cellLookupService.registrationComplete();
      if (!dataset || !ready) {
        return;
      }

      const selectedCells: PUSpecSheetTableCellBase[] = [];
      for (const column of dataset.columns) {
        const cell = this._cellLookupService.getCell(column.columnKey);
        if (!cell) {
          continue;
        }

        selectedCells.push(cell as PUSpecSheetTableCellBase);
      }

      this._selectionService.select(...selectedCells);
    });

    effect(() => {
      const selectedCells = this._selectionService.selectedCells();
      const dataset = untracked(() => this._analyticsService.currentDataset());
      if (!dataset) {
        return;
      }

      const sourceConfiguration = this.getCurrentSourceConfiguration(dataset);
      const sourceConfigurationChanged = this.hasSourceConfigurationChanged(
        dataset,
        sourceConfiguration,
      );
      const sourceConfigurationUpdateRequired =
        this.hasSourceConfigurationUpdateRequired(dataset, sourceConfiguration);
      if (sourceConfigurationChanged && dataset.columns.length > 0) {
        const sourceConfigurationChangeKey =
          this.getSourceConfigurationChangeKey(dataset, sourceConfiguration);
        if (
          AnalyticsDataPointConfiguration._sourceChangeConfirmationOpen ||
          AnalyticsDataPointConfiguration._lastPromptedSourceConfigurationChangeKey ===
            sourceConfigurationChangeKey
        ) {
          return;
        }

        AnalyticsDataPointConfiguration._lastPromptedSourceConfigurationChangeKey =
          sourceConfigurationChangeKey;
        void this.confirmSourceConfigurationChangeAsync(
          dataset,
          sourceConfiguration,
        );
        return;
      }

      AnalyticsDataPointConfiguration._lastPromptedSourceConfigurationChangeKey =
        null;

      if (this.hasMatchingDataPointReferences(dataset, selectedCells)) {
        if (sourceConfigurationUpdateRequired) {
          const updatedDataset: Dataset = {
            ...dataset,
            sourceConfiguration,
          };
          this._analyticsService.currentDataset.set(updatedDataset);
          void this.sendDatasetUpdateAsync(updatedDataset);
        }
        return;
      }

      const dataPoints: DatasetColumn[] = [];
      for (const selectedCell of selectedCells) {
        const existingDataPoint = dataset.columns.find(
          (dp) => dp.columnKey === selectedCell.referenceId(),
        );
        if (existingDataPoint) {
          dataPoints.push(existingDataPoint);
          continue;
        }

        const newDataPoint: DatasetColumn = {
          id: 0,
          columnKey: selectedCell.referenceId(),
          displayName: selectedCell.tableCellType().title,
          displayOrder: dataset.columns.length + 1,
          dataType: this.mapFunctionDataTypeToDatasetColumnType(
            selectedCell.tableCellType(),
          ),
          configuration: null,
          formula: null,
          isFormula: false,
          isMetadata: false,
        };
        dataPoints.push(newDataPoint);
      }

      const updatedDataset: Dataset = {
        ...dataset,
        columns: dataPoints,
        sourceConfiguration: {
          ...dataset.sourceConfiguration,
          ...sourceConfiguration,
        },
      };
      this._analyticsService.currentDataset.set(updatedDataset);
      void this.sendDatasetUpdateAsync(updatedDataset);
    });
  }

  public onDataPointRemoved(dataPoint: DatasetColumn): void {
    const dataset = this._analyticsService.currentDataset();
    if (!dataset) {
      return;
    }

    dataset.columns = dataset.columns.filter(
      (dp) => dp.columnKey !== dataPoint.columnKey,
    );
    this._analyticsService.currentDataset.set({ ...dataset });

    this._datasetCommunicationService
      .sendDatasetUpdateToParent(dataset)
      .catch((error) => {
        console.error(
          'AnalyticsDataPointConfigurationComponent: Failed to send dataset update',
          error,
        );
      });
  }

  public get dataset(): Signal<Dataset | null> {
    return this._analyticsService.currentDataset;
  }

  private hasMatchingDataPointReferences(
    dataset: Dataset,
    selectedCells: PUSpecSheetTableCellBase[],
  ): boolean {
    if (dataset.columns.length !== selectedCells.length) {
      return false;
    }

    return dataset.columns.every(
      (dataPoint, index) =>
        dataPoint.columnKey === selectedCells[index]?.referenceId(),
    );
  }

  private getCurrentSourceConfiguration(
    dataset: Dataset,
  ): PUSpecSheetSourceConfiguration {
    return {
      ...dataset.sourceConfiguration,
      typeId:
        this.specSheetTypeSelected()?.id ?? dataset.sourceConfiguration.typeId,
      phaseId:
        this.specSheetPhaseSelected()?.id ??
        dataset.sourceConfiguration.phaseId,
      useLatestVersion: this.useLatestVersion(),
      puSpecSheetId: this.useLatestVersion()
        ? null
        : (this._activeSheetService.specSheet()?.id ??
          dataset.sourceConfiguration.puSpecSheetId),
    };
  }

  private hasSourceConfigurationChanged(
    dataset: Dataset,
    sourceConfiguration: PUSpecSheetSourceConfiguration,
  ): boolean {
    return (
      dataset.sourceConfiguration.typeId !== sourceConfiguration.typeId ||
      dataset.sourceConfiguration.phaseId !== sourceConfiguration.phaseId
    );
  }

  private hasSourceConfigurationUpdateRequired(
    dataset: Dataset,
    sourceConfiguration: PUSpecSheetSourceConfiguration,
  ): boolean {
    return (
      this.hasSourceConfigurationChanged(dataset, sourceConfiguration) ||
      dataset.sourceConfiguration.useLatestVersion !==
        sourceConfiguration.useLatestVersion ||
      dataset.sourceConfiguration.puSpecSheetId !==
        sourceConfiguration.puSpecSheetId
    );
  }

  private async confirmSourceConfigurationChangeAsync(
    dataset: Dataset,
    sourceConfiguration: PUSpecSheetSourceConfiguration,
  ): Promise<void> {
    if (AnalyticsDataPointConfiguration._sourceChangeConfirmationOpen) {
      return;
    }

    AnalyticsDataPointConfiguration._sourceChangeConfirmationOpen = true;

    try {
      const dialogRef = this._dialog.open<
        boolean,
        ConfirmationDialogData,
        ConfirmationDialog
      >(ConfirmationDialog, {
        data: {
          title: 'Change Analytics Source',
          message:
            'Changing the spec sheet source settings will wipe all analytics data points from this dataset. Do you want to continue?',
          confirmText: 'Continue',
          confirmSeverity: 'danger',
        },
      });

      const confirmed = await firstValueFrom(dialogRef.closed);
      if (!confirmed) {
        this.useLatestVersion.set(
          dataset.sourceConfiguration.useLatestVersion ?? false,
        );
        return;
      }

      const updatedDataset: Dataset = {
        ...dataset,
        columns: [],
        sourceConfiguration,
      };
      this._analyticsService.currentDataset.set(updatedDataset);
      this._selectionService.reset();
      await this.sendDatasetUpdateAsync(updatedDataset);
    } finally {
      AnalyticsDataPointConfiguration._sourceChangeConfirmationOpen = false;
    }
  }

  private getSourceConfigurationChangeKey(
    dataset: Dataset,
    sourceConfiguration: PUSpecSheetSourceConfiguration,
  ): string {
    return [
      dataset.id,
      dataset.refId,
      dataset.sourceConfiguration.typeId,
      dataset.sourceConfiguration.phaseId,
      sourceConfiguration.typeId,
      sourceConfiguration.phaseId,
      sourceConfiguration.useLatestVersion,
      sourceConfiguration.puSpecSheetId,
    ].join('|');
  }

  private async sendDatasetUpdateAsync(dataset: Dataset): Promise<void> {
    try {
      await this._datasetCommunicationService.sendDatasetUpdateToParent(
        dataset,
      );
    } catch (error) {
      console.error(
        'AnalyticsDataPointConfigurationComponent: Failed to send dataset update',
        error,
      );
    }
  }

  private mapFunctionDataTypeToDatasetColumnType(
    cellType: TableCellType | undefined,
  ): DatasetColumnType {
    switch (cellType?.modelType) {
      case 'text':
      case undefined:
        return DatasetColumnType.String;
      case 'numeric':
        return DatasetColumnType.Double;
      case 'date':
        return DatasetColumnType.DateTime;
      case 'boolean':
        return DatasetColumnType.Bool;
      default:
        return DatasetColumnType.String;
    }
  }
}
