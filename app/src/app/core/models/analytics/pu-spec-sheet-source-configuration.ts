export class PUSpecSheetSourceConfiguration {
  public readonly $type: string = 'puSpecSheet';
  public typeId: number | null;
  public phaseId: number | null;
  public useLatestVersion: boolean | null;
  public puSpecSheetId: number | null;

  constructor(
    typeId: number | null,
    phaseId: number | null,
    useLatestVersion: boolean | null,
    puSpecSheetId: number | null,
  ) {
    this.typeId = typeId;
    this.phaseId = phaseId;
    this.useLatestVersion = useLatestVersion;
    this.puSpecSheetId = puSpecSheetId;
  }
}
