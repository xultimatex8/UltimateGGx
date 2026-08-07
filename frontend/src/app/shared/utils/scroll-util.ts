export class ScrollUtil {
  private static readonly DEFAULT_OFFSET = 80;

  static scrollTo(
    event: MouseEvent | null,
    id: string,
    offset: number = ScrollUtil.DEFAULT_OFFSET,
  ): void {
    event?.preventDefault();

    const element = document.getElementById(id);
    if (!element) return;

    const y =
      element.getBoundingClientRect().top +
      window.scrollY -
      offset;

    window.scrollTo({
      top: y,
      behavior: 'smooth',
    });

    history.replaceState(null, '', `#${id}`);
  }
}