import mawasemLogo from "@/assets/mawasem Logo.png";

export default function HomePage() {
  return (
    <section className="relative min-h-screen overflow-hidden bg-[linear-gradient(180deg,#f8fafc_0%,#ffffff_45%,#f6f7fb_100%)] text-slate-900">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_rgba(15,23,42,0.08),_transparent_36%),radial-gradient(circle_at_bottom_right,_rgba(148,163,184,0.16),_transparent_30%)]" />
      <div className="absolute inset-x-0 top-0 h-24 bg-gradient-to-b from-white/90 to-transparent" />

      <div className="relative flex min-h-screen items-center justify-center px-4 py-10 sm:px-6 lg:px-8">
        <div className="w-full max-w-4xl animate-in fade-in-0 zoom-in-95 duration-700">
          <div className="mx-auto mb-6 flex w-fit items-center gap-2 rounded-full border border-slate-200 bg-white/80 px-4 py-2 shadow-sm backdrop-blur-sm animate-in fade-in-0 slide-in-from-top-3 duration-500">
            <div className="size-2 rounded-full bg-emerald-500 shadow-[0_0_0_6px_rgba(16,185,129,0.12)]" />
            <span className="text-xs font-semibold uppercase tracking-[0.35em] text-slate-500">
              Mawasem
            </span>
          </div>

          <div className="mx-auto max-w-3xl rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-[0_30px_100px_-50px_rgba(15,23,42,0.35)] backdrop-blur-xl sm:p-10 lg:p-14">
            <div className="text-center">
              <div className="mb-7 flex justify-center">
                <div className="rounded-[1.75rem] border border-slate-200 bg-white px-4 py-3 shadow-sm sm:px-5 sm:py-4">
                  <img
                    src={mawasemLogo}
                    alt="Mawasem logo"
                    className="h-20 w-auto sm:h-24"
                  />
                </div>
              </div>

              <p className="mb-4 text-xs font-semibold uppercase tracking-[0.4em] text-slate-400 animate-in fade-in-0 slide-in-from-bottom-3 duration-500">
                A better learning experience is on the way
              </p>

              <h1 className="font-heading text-5xl font-semibold tracking-tight text-slate-950 sm:text-6xl lg:text-7xl animate-in fade-in-0 slide-in-from-bottom-4 duration-700">
                Coming Soon
              </h1>

              <div className="mx-auto mt-6 max-w-2xl space-y-4 text-base leading-8 text-slate-600 sm:text-lg animate-in fade-in-0 slide-in-from-bottom-4 duration-700 delay-150">
                <p>
                  We&apos;re building something exceptional for engineers,
                  educators, and learners across the Arab world.
                </p>
                <p>
                  Our new platform is currently under development and will be
                  available soon.
                </p>
              </div>
            </div>

            <div className="mx-auto mt-10 max-w-2xl animate-in fade-in-0 slide-in-from-bottom-4 duration-700 delay-200">

              <footer className="text-center text-sm text-slate-500 animate-in fade-in-0 duration-700 delay-300">
                © 2026 Mawasem. All rights reserved.
              </footer>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
