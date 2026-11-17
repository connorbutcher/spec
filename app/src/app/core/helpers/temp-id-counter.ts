let tempIdCounter = 0;

export function nextTempId(): number {
  tempIdCounter -= 1;
  return tempIdCounter;
}
